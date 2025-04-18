<template>
  <div>
    <div class="tagslist-toolbar" style="width: 99%;">
      <el-input v-model="filterText" placeholder="输入关键字过滤" style="width: 300px;" />
      <div class="tagslist-toolbar-btns">
        <el-button type="success" @click="onSaveTags">保存点表</el-button>
        <el-button type="primary" @click="onLoadTags">加载点表</el-button>
        <el-button type="info" @click="onLoadCSV">加载CSV</el-button>
      </div>
    </div>
    <div ref="tableWrapper" style="width: 99%;">
      <ElTableV2
        :columns="columns"
        :data="sortedData"
        :width="tableWidth"
        :height="500"
        style="border: 1px solid #ebeef5;"
        :sort-by="sortBy"
        :sort-direction="sortDirection"
        @sort="handleSort"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick, inject, onBeforeUnmount, defineExpose } from 'vue';
import { ElTableV2, ElMessage } from 'element-plus';
import type { AnyColumn } from 'element-plus/es/components/table-v2/src/common';

// 1. 删除模拟数据，定义表格数据结构
type TagRow = {
  name: string;
  desc: string;
  type: string;
  opc_item: string;
};
const data = ref<TagRow[]>([]);

// 获取全局 restartFlag
const restartFlag = inject<{ value: boolean }>('restartFlag'); // 注入变量
// 2. 获取全局 mqttClient
const mqttClient = inject<any>('mqttClient');
let mqttClientId = '';
onMounted(() => {
  mqttClientId = (window as any).mqttClientId || '';
});

let responseHandler: any = null;

// 3. 过滤逻辑
const filterText = ref('');
const filteredData = computed(() =>
  data.value.filter((item) =>
    item.name.toLowerCase().includes(filterText.value.toLowerCase()) ||
    item.desc.toLowerCase().includes(filterText.value.toLowerCase()) ||
    item.type.toLowerCase().includes(filterText.value.toLowerCase()) ||
    item.opc_item.toLowerCase().includes(filterText.value.toLowerCase())
  )
);

// 排序相关逻辑
const sortBy = ref<any>('name'); // 修正类型为 any
const sortDirection = ref<'ASC' | 'DESC'>('ASC');

const handleSort = (params: { key: string; order: 'ASC' | 'DESC' }) => {
  sortBy.value = params.key;
  sortDirection.value = params.order;
};

const sortedData = computed(() => {
  const arr = filteredData.value.slice();
  if (!sortBy.value) return arr;
  return arr.sort((a: any, b: any) => {
    const valA = a[sortBy.value];
    const valB = b[sortBy.value];
    if (valA == null && valB == null) return 0;
    if (valA == null) return sortDirection.value === 'ASC' ? -1 : 1;
    if (valB == null) return sortDirection.value === 'ASC' ? 1 : -1;
    if (valA < valB) return sortDirection.value === 'ASC' ? -1 : 1;
    if (valA > valB) return sortDirection.value === 'ASC' ? 1 : -1;
    return 0;
  });
});

// 4. el-table-v2 列定义，增加 sortable 属性
const columns: AnyColumn[] = [
  { key: 'name', dataKey: 'name', title: '点名', minWidth: 120, width: 140, flexGrow: 1, sortable: true },
  { key: 'desc', dataKey: 'desc', title: '描述', minWidth: 120, width: 160, flexGrow: 1, sortable: true },
  { key: 'type', dataKey: 'type', title: '类型', minWidth: 80, width: 100, flexGrow: 1, sortable: true },
  { key: 'opc_item', dataKey: 'opc_item', title: 'OPC_item', minWidth: 180, width: 200, flexGrow: 2, sortable: true }
];

// 5. 动态获取容器宽度，传递给 :width
const tableWidth = ref(800);
const tableWrapper = ref<HTMLElement | null>(null);

const updateTableWidth = () => {
  nextTick(() => {
    if (tableWrapper.value) {
      tableWidth.value = tableWrapper.value.offsetWidth;
    }
  });
};

onMounted(() => {
  updateTableWidth();
  window.addEventListener('resize', updateTableWidth);
});

onBeforeUnmount(() => {
  if (mqttClient && mqttClient.value && responseHandler) {
    mqttClient.value.off('message', responseHandler);
    responseHandler = null;
  }
});

// 6. 生成随机 reqid
function randomReqId() {
  return 'web' + Math.floor(Math.random() * 1000000);
}

// 7. 加载点表
function onLoadTags() {
  if (!mqttClient || !mqttClient.value) {
    ElMessage.error('mqttClient 未连接，请检查连接状态！');
    return;
  }
  mqttClientId = (window as any).mqttClientId || mqttClient.value.options?.clientId || mqttClient.value.options?.clientid || '';
  if (!mqttClientId) {
    ElMessage.error('无法获取 MQTT ClientId');
    return;
  }
  const reqid = randomReqId();
  const topicCmd = `client/${mqttClientId}/command`;
  const topicResp = `client/${mqttClientId}/response`;
  const payload = JSON.stringify({
    cmd: 'get',
    type: 'tags',
    data: '',
    reqid
  });

  // 订阅响应主题
  mqttClient.value.subscribe(topicResp);

  // 响应处理
  if (responseHandler) {
    mqttClient.value.off('message', responseHandler);
  }
  responseHandler = (topic: string, message: Uint8Array) => {
    if (topic === topicResp) {
      try {
        const resp = JSON.parse(message.toString());
        if (resp.reqid === reqid && resp.result && Array.isArray(resp.data)) {
          // 清空当前表格数据
          data.value = [];
          // 解析并赋值
          for (const row of resp.data) {
            if (Array.isArray(row) && row.length >= 4) {
              data.value.push({
                name: row[0] ?? '',
                desc: row[1] ?? '',
                type: row[2] ?? '',
                opc_item: row[3] ?? ''
              });
            }          
          }
          ElMessage.success('点表加载成功！');
        }
      } catch (e) {
        ElMessage.error("异常: " + (e as Error).message);
      }
    }
  };
  mqttClient.value.on('message', responseHandler);

  // 发送请求
  mqttClient.value.publish(topicCmd, payload);
}

function onSaveTags() {
  if (!mqttClient || !mqttClient.value) {
    ElMessage.error('mqttClient 未连接，请检查连接状态！');
    return;
  }
  if (!data.value.length) {
    ElMessage.warning('表格数据为空，无法保存！');
    return;
  }
  mqttClientId = (window as any).mqttClientId || mqttClient.value.options?.clientId || mqttClient.value.options?.clientid || '';
  if (!mqttClientId) {
    ElMessage.error('无法获取 MQTT ClientId');
    return;
  }
  const reqid = randomReqId();
  const topicCmd = `client/${mqttClientId}/command`;
  const topicResp = `client/${mqttClientId}/response`;
  // 构造要保存的数据
  const tagsArray = data.value.map(row => [
    row.name,
    row.desc,
    row.type,
    row.opc_item
  ]);
  const payload = JSON.stringify({
    cmd: 'set',
    type: 'tags',
    data: tagsArray,
    reqid
  });

  // 订阅响应主题
  mqttClient.value.subscribe(topicResp);

  // 响应处理
  if (responseHandler) {
    mqttClient.value.off('message', responseHandler);
  }
  responseHandler = (topic: string, message: Uint8Array) => {
    if (topic === topicResp) {
      try {
        const resp = JSON.parse(message.toString());
        if (resp.reqid === reqid) {
          if (resp.result) {
            ElMessage.success('点表保存成功！需要重启后端服务才能生效！');
            if (restartFlag) {
              restartFlag.value = true;
            }
          } else {
            ElMessage.error('点表保存失败: ' + (resp.message || '未知错误'));
          }
        }
      } catch (e) {
        ElMessage.error("异常: " + (e as Error).message);
      }
    }
  };
  mqttClient.value.on('message', responseHandler);

  // 发送请求
  mqttClient.value.publish(topicCmd, payload);
}


function onLoadCSV() {
  const input = document.createElement('input');
  input.type = 'file';
  input.accept = '.csv,text/csv';
  input.style.display = 'none';
  document.body.appendChild(input);
  input.onchange = async (event: Event) => {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) {
      document.body.removeChild(input);
      return;
    }
    const reader = new FileReader();
    reader.onload = async (e) => {
      const buffer = e.target?.result as ArrayBuffer;
      if (!buffer) {
        document.body.removeChild(input);
        return;
      }
      // 尝试 UTF-8 解码
      let text = new TextDecoder('utf-8').decode(buffer);
      
      // 如果 UTF-8 解码失败（乱码），尝试 GBK
      if (isGarbled(text)) {
        try {
          const gbkDecoder = new TextDecoder('gbk');
          text = gbkDecoder.decode(buffer);
        } catch (err) {
          ElMessage.error('无法解码 GBK 格式文件');
          document.body.removeChild(input);
          return;
        }
      }
      // 解析 CSV
      const lines = text.split(/\r?\n/).map(line => line.trim()).filter(line => line.length > 0);
      if (lines.length < 2) {
        ElMessage.warning('CSV内容为空或无有效数据');
        document.body.removeChild(input);
        return;
      }
      // 检查表头
      const header = lines[0].split(',').map(h => h.trim());
      if (
        header.length < 4 ||
        header[0] !== '点名' ||
        header[1] !== '描述' ||
        header[2] !== '类型' ||
        (header[3] !== 'OPC_Item' && header[3] !== 'OPC_item')
      ) {
        ElMessage.error('CSV表头格式不正确，需为: 点名,描述,类型,OPC_Item');
        document.body.removeChild(input);
        return;
      }
      // 解析数据
      data.value = [];
      for (let i = 1; i < lines.length; i++) {
        const row = lines[i].split(',').map(cell => cell.trim());
        if (row.length < 4) continue;
        data.value.push({
          name: row[0],
          desc: row[1],
          type: row[2],
          opc_item: row[3]
        });
      }
      ElMessage.success('CSV加载成功');
      document.body.removeChild(input);
    };
    // 以 ArrayBuffer 形式读取文件
    reader.readAsArrayBuffer(file);
  };
  input.click();
}
/** 检测文本是否乱码 */
function isGarbled(text: string): boolean {
  // 简单逻辑：如果包含大量 � 或非法字符，可能是 GBK
  return text.includes('�') || /[^\u0000-\uFFFF]/.test(text);
}


// 提供给父组件获取表格数据
function getTableData() {
  console.log('[TagsList.vue] getTableData', data.value);
  return data.value;
}

// 提供给父组件设置表格数据
function setTableData(arr: any[]) {
  console.log('[TagsList.vue] setTableData', arr);
  if (Array.isArray(arr)) {
    data.value = arr.map(row => ({
      name: row.name ?? '',
      desc: row.desc ?? '',
      type: row.type ?? '',
      opc_item: row.opc_item ?? ''
    }));
  }
}

// 暴露方法给父组件
defineExpose({ getTableData, setTableData });
</script>

<style scoped>
.tagslist-toolbar {
  display: flex;
  align-items: center;
  margin-bottom: 10px;
  width: 100%;
}
.tagslist-toolbar-btns {
  margin-left: auto;
  display: flex;
  gap: 10px;
}
</style>