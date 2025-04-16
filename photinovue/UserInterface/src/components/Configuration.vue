<script setup lang="ts">
import { ref } from 'vue';

const opcHost = ref('localhost');
const opcProgId = ref('');
const tdHost = ref('localhost');
const tdPort = ref('');
const tdDbName = ref('');
const tdUsername = ref('');
const tdPassword = ref('');

function saveConfig() {
  // 保存配置逻辑
}

function resetConfig() {
  // 重置配置逻辑
}
</script>

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
      <button @click="saveConfig">保存配置</button>
      <button @click="resetConfig">重置配置</button>
    </div>
  </div>
</template>

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
  margin-bottom: 16px;
}

.button-row {
  display: flex;
  gap: 10px;
  margin-top: 20px;
}

.button-row button {
  padding: 10px 20px;
  background-color: #4CAF50;
  color: white;
  border: none;
  cursor: pointer;
}

.button-row button:hover {
  background-color: #45a049;
}
</style>