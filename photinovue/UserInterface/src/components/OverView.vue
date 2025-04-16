<template>
  <div class="overview-container">
    <h2>后台服务状态</h2>
    <!-- 使用 Flexbox 实现左右排列 -->
    <div class="service-row">
      <div class="service-status">
        <h3>OPC采集服务</h3>
        <p>服务状态：</p>
      </div>
      <div class="service-status">
        <h3>TdEngine写数服务</h3>
        <p>服务状态：</p>
      </div>
    </div>
    <div class="log-view">
      <h3>程序日志：</h3>
      <textarea readonly rows="15" v-model="logContent"></textarea>
      <button @click="clearLog">清除</button>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';

const logContent = ref('');

function clearLog() {
  logContent.value = '';
}
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