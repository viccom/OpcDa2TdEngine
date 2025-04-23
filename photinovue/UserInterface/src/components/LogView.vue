
<template>
  <div>
    <div class="tagslist-toolbar" style="width: 99%;">
      <el-input v-model="filterText" placeholder="输入关键字过滤" style="width: 300px;" />
      <div class="tagslist-toolbar-btns">
        <el-button
          :type= "logSubscribed ? 'warning' : 'success'"
          @click="handleLogSub">
          {{ logSubscribed ? '取消订阅' : '订阅日志' }}
        </el-button>
        <el-button type="success" @click="clearLog">
          清空日志
        </el-button>
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
import { ref, computed, onMounted, nextTick, inject, onBeforeUnmount, Ref } from 'vue'
import { ElTableV2 } from 'element-plus';
import type { AnyColumn } from 'element-plus/es/components/table-v2/src/common';

// 使用 inject 注入 mqttClient
const mqttClient = inject<any>('mqttClient');

const logdata = inject<any>('logdata', ref([]));
// 获取全局 restartFlag
const logSubscribed = inject<Ref<boolean>>('logSubscribed'); // 注入变量

// 使用 inject 注入 handleLogSub 函数
const handleLogSub = inject<() => void>('handleLogSub', () => {
  console.error('handleLogSub 未注入');
});

// 3. 过滤逻辑
const filterText = ref('');
const filteredData = computed(() =>
  logdata.value.filter((item) =>
    item.ts.toLowerCase().includes(filterText.value.toLowerCase()) ||
    item.level.toLowerCase().includes(filterText.value.toLowerCase()) ||
    item.module.toLowerCase().includes(filterText.value.toLowerCase()) ||
    item.message.toLowerCase().includes(filterText.value.toLowerCase())
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
  { key: 'ts', dataKey: 'ts', title: '时间', minWidth: 120, width: 140, flexGrow: 1, sortable: true },
  { key: 'level', dataKey: 'level', title: '等级', minWidth: 80, width: 100, flexGrow: 1, sortable: true },
  { key: 'module', dataKey: 'module', title: '模块', minWidth: 80, width: 100, flexGrow: 1, sortable: true },
  { key: 'message', dataKey: 'message', title: '日志', minWidth: 280, width: 280, flexGrow: 2, sortable: false }
];

// 5. 动态获取容器宽度，传递给 :width
const tableWidth = ref(800);
const tableWrapper = ref<HTMLElement | null>(null);

const updateTableWidth = () => {
  nextTick(() => {
    if (tableWrapper.value) {
      tableWidth.value = tableWrapper.value.offsetWidth || 800; // 防止空引用
    }
  });
};

onMounted(() => {
  updateTableWidth();
  window.addEventListener('resize', updateTableWidth);
});

onBeforeUnmount(() => {

});

// 新增 clearLog 方法
const clearLog = () => {
  logdata.value = []; // 清空日志数据
};
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