<template>
  <div class="configuration-container">

    <!-- 使用 Flexbox 实现左右排列 -->
    <div class="service-row">
      <div class="service-status" style="margin-right: 20px;">
        <h3>OPC DA 服务器：</h3>
        <div class="input-row">
          <label for="opc-host">Host：</label>
          <input id="opc-host" v-model="opcHost" type="text" placeholder="localhost">
        </div>
        <div class="input-row">
          <label for="opc-progid">ProgID：</label>
          <input id="opc-progid" v-model="opcProgId" type="text">
        </div>
      </div>
      <div class="service-status">
        <h3>TDengine 数据库：</h3>
        <div class="input-row">
          <label for="td-host">Host：</label>
          <input id="td-host" v-model="tdHost" type="text" placeholder="localhost">
        </div>
        <div class="input-row">
          <label for="td-port">Port：</label>
          <input id="td-port" v-model="tdPort" type="text">
        </div>
        <div class="input-row">
          <label for="td-dbname">DbName：</label>
          <input id="td-dbname" v-model="tdDbName" type="text">
        </div>
        <div class="input-row">
          <label for="td-username">UserName：</label>
          <input id="td-username" v-model="tdUsername" type="text">
        </div>
        <div class="input-row">
          <label for="td-password">Password：</label>
          <input id="td-password" v-model="tdPassword" type="password">
        </div>
      </div>
    </div>

    <!-- 按钮区域 -->
    <div class="button-row">
      <el-button type="success" @click="saveConfig">保存配置</el-button>
      <el-button type="primary" @click="loadConfig">加载配置</el-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, inject, onMounted, onBeforeUnmount, defineExpose } from 'vue';
import { ElMessage } from 'element-plus';

const opcHost = ref('localhost');
const opcProgId = ref('');
const tdHost = ref('localhost');
const tdPort = ref('');
const tdDbName = ref('');
const tdUsername = ref('');
const tdPassword = ref('');

import type { Ref } from 'vue';
// 获取全局 restartFlag
const restartFlag = inject<Ref<boolean>>('restartFlag'); // 注入变量
// 获取全局 mqttClient
const mqttClient = inject<any>('mqttClient');
// 获取全局 mqttClientId（App.vue 里 clientid 生成后可挂到 window 或 provide）
let mqttClientId = '';
onMounted(() => {
  // 尝试从 window 取 clientid
  mqttClientId = (window as any).mqttClientId || '';
  // 或者你可以通过 provide/inject 方式获取
});

let responseHandler: any = null;

function randomReqId() {
  return 'web' + Math.floor(Math.random() * 1000000);
}

function saveConfig() {
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
    cmd: 'set',
    type: 'config',
    data: {
      OpcDa: {
        Host: opcHost.value,
        ProgID: opcProgId.value
      },
      TdEngine: {
        Host: tdHost.value,
        Port: tdPort.value ? Number(tdPort.value) : '',
        Dbname: tdDbName.value,
        Username: tdUsername.value,
        Password: tdPassword.value
      }
    },
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
            ElMessage.success('配置保存成功！需要重启后端服务才能生效！');
            if (restartFlag) {
              restartFlag.value = true;
            }
          } else {
            ElMessage.error('配置保存失败: ' + (resp.message || '未知错误'));
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

function loadConfig() {
  console.log('[Configuration.vue] loadConfig 被调用');
  if (!mqttClient || !mqttClient.value) {
    ElMessage.error('mqttClient 未连接，请检查连接状态！');
    return;
  }
  // 获取 clientid
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
    type: 'config',
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
        if (resp.reqid === reqid && resp.result && resp.data) {
          // 解析并赋值
          if (resp.data.OpcDa) {
            opcHost.value = resp.data.OpcDa.Host ?? '';
            opcProgId.value = resp.data.OpcDa.ProgID ?? '';
          }
          if (resp.data.TdEngine) {
            tdHost.value = resp.data.TdEngine.Host ?? '';
            tdPort.value = resp.data.TdEngine.Port?.toString() ?? '';
            tdDbName.value = resp.data.TdEngine.Dbname ?? '';
            tdUsername.value = resp.data.TdEngine.Username ?? '';
            tdPassword.value = resp.data.TdEngine.Password ?? '';
          }
          if (resp.result) {
            ElMessage.success('配置加载成功！');
          } else {
            ElMessage.error('配置加载失败: ' + (resp.message || '未知错误'));
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

// 提供给父组件获取表单数据
function getFormData() {
  const result = {
    opcHost: opcHost.value,
    opcProgId: opcProgId.value,
    tdHost: tdHost.value,
    tdPort: tdPort.value,
    tdDbName: tdDbName.value,
    tdUsername: tdUsername.value,
    tdPassword: tdPassword.value
  };
  console.log('[Configuration.vue] getFormData', result);
  return result;
}

// 提供给父组件设置表单数据
function setFormData(data: any) {
  console.log('[Configuration.vue] setFormData', data);
  opcHost.value = data.opcHost ?? '';
  opcProgId.value = data.opcProgId ?? '';
  tdHost.value = data.tdHost ?? '';
  tdPort.value = data.tdPort ?? '';
  tdDbName.value = data.tdDbName ?? '';
  tdUsername.value = data.tdUsername ?? '';
  tdPassword.value = data.tdPassword ?? '';
}

// 暴露方法给父组件
defineExpose({ getFormData, setFormData, loadConfig });

onBeforeUnmount(() => {
  if (mqttClient && mqttClient.value && responseHandler) {
    mqttClient.value.off('message', responseHandler);
    responseHandler = null;
  }
});
</script>



<style scoped>
.configuration-container {
  padding: 20px;
}

/* Flexbox 布局实现左右排列 */
.service-row {
  display: flex;
  gap: 20px; /* 设置间距 */
  margin-bottom: 20px;
}

.service-status {
  border: 1px solid #e0e0e0;
  padding: 20px;
  min-height: 120px;
  flex: 1; /* 让每个卡片占据相等宽度 */
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  justify-content: space-between; /* 让内容均匀分布 */
}

.input-row {
  display: flex;
  align-items: center;
  margin-bottom: 16px;
}

.input-row label {
  display: inline-block;
  width: 90px;
  min-width: 90px;
  text-align: right;
  margin-right: 8px;
}

.input-row input {
  flex: 1;
  min-width: 100px;
  padding: 6px 8px;
  box-sizing: border-box;
}

.button-row {
  display: flex;
  gap: 10px;
  margin-top: 20px;
}

.button-row button {
  padding: 10px 20px;
  color: white;
  border: none;
  cursor: pointer;
}

.button-row button:hover {
  background-color: #45a049;
}
</style>