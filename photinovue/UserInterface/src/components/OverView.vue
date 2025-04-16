<template>
  <div class="overview-container">
    <h2>后台服务状态</h2>
    <!-- 使用 Flexbox 实现左右排列 -->
    <div class="service-row">
      <div class="service-status">
        <h3>OPC采集服务</h3>
        <p>
          服务状态：
          <span :class="opcDaSubStatus ? 'status-running' : 'status-stopped'">
            {{ opcDaSubStatus ? '运行中' : '已停止' }}
          </span>
        </p>
      </div>
      <div class="service-status">
        <h3>TdEngine写数服务</h3>
        <p>
          服务状态：
          <span :class="tdEnginePubStatus ? 'status-running' : 'status-stopped'">
            {{ tdEnginePubStatus ? '运行中' : '已停止' }}
          </span>
        </p>
      </div>
    </div>
    <div class="log-view">
      <h3>页面日志：</h3>
      <textarea readonly rows="15" v-model="logContent"></textarea>
      <button @click="clearLog">清除</button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue';

const logContent = ref('');
const opcDaSubStatus = ref(false);
const tdEnginePubStatus = ref(false);

let client = null;
let checkTimer = null;


function clearLog() {
  logContent.value = '';
}

function connectMqtt() {
  if (!window.mqtt) {
    alert('未检测到 MQTT 客户端库，请确认已在 index.html 中通过 CDN 引入 mqtt.min.js');
    return;
  }
  // 断开旧连接
  if (client) {
    client.end(true);
    client = null;
  }
  // 设置自动重连参数和用户名密码
  client = window.mqtt.connect('ws://localhost:6882/mqtt', {
    reconnectPeriod: 5000, // 5秒自动重连
    username: 'admin',
    password: '123456'
  });
  client.on('connect', () => {
    client.subscribe('/status/apps');
    logContent.value += '[MQTT] 已连接\n';
  });
  client.on('reconnect', () => {
    logContent.value += '[MQTT] 正在重连...\n';
  });
  client.on('close', () => {
    logContent.value += '[MQTT] 连接已关闭\n';
  });
  client.on('offline', () => {
    logContent.value += '[MQTT] 已离线\n';
  });
  client.on('error', (err) => {
    logContent.value += '[MQTT] 错误: ' + err?.message + '\n';
  });
  client.on('message', (topic, message) => {
    if (topic === '/status/apps') {
      try {
        const data = JSON.parse(message.toString());
        opcDaSubStatus.value = !!data.opcDaSub;
        tdEnginePubStatus.value = !!data.tdEnginePub;
      } catch (e) {
        logContent.value += '[MQTT] 消息解析失败\n';
      }
    }
  });
}

onMounted(() => {
  connectMqtt();
  // 周期检测连接状态，断线时尝试重连
  checkTimer = setInterval(() => {
    if (!client || !client.connected) {
      logContent.value += '[MQTT] 检测到断线，尝试重连...\n';
      connectMqtt();
    }
  }, 10000); // 每10秒检测一次
});

onBeforeUnmount(() => {
  if (checkTimer) {
    clearInterval(checkTimer);
    checkTimer = null;
  }
  if (client) {
    client.end();
    client = null;
  }
});
</script>

<style scoped>
.overview-container {
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

.status-running {
  color: #4caf50;
  font-weight: bold;
}

.status-stopped {
  color: #f44336;
  font-weight: bold;
}

.log-view {
  border: 1px solid #e0e0e0;
  padding: 20px;
}

.log-view textarea {
  width: 100%;
  height: 200px;
  resize: none;
  margin-bottom: 10px;
}

.log-view button {
  float: right;
}
</style>