
<template>
  <div class="overview-container">
    <h2>后台服务状态</h2>
    <!-- <div style="margin-bottom: 16px;">
      <label>
        OPC采集服务原始值：
        <input
          type="text"
          readonly
          :value="overViewStatus?.opcDaSub === true ? 'true' : (overViewStatus?.opcDaSub === false ? 'false' : '--')"
          style="width:80px;margin-right:24px;"
        />
      </label>
      <label>
        TdEngine写数服务原始值：
        <input
          type="text"
          readonly
          :value="overViewStatus?.tdEnginePub === true ? 'true' : (overViewStatus?.tdEnginePub === false ? 'false' : '--')"
          style="width:80px;"
        />
      </label>
    </div> -->
    <div class="service-row">
      <div class="service-status">
        <h3>OPC采集服务</h3>
        <p>
          服务状态：
          <span :class="overViewStatus?.opcDaSub === true ? 'status-running' : (overViewStatus?.opcDaSub === false ? 'status-stopped' : '')">
            {{ overViewStatus?.opcDaSub === true ? '运行中' : (overViewStatus?.opcDaSub === false ? '已停止' : '--') }}
          </span>
        </p>
      </div>
      <div class="service-status">
        <h3>TdEngine写数服务</h3>
        <p>
          服务状态：
          <span :class="overViewStatus?.tdEnginePub === true ? 'status-running' : (overViewStatus?.tdEnginePub === false ? 'status-stopped' : '')">
            {{ overViewStatus?.tdEnginePub === true ? '运行中' : (overViewStatus?.tdEnginePub === false ? '已停止' : '--') }}
          </span>
        </p>
      </div>
    </div>

  </div>
</template>



<script setup lang="ts">
// 声明 window.mqtt
declare global {
  interface Window {
    mqtt: any;
  }
}

import { ref, inject, watch } from 'vue'
import type { Ref } from 'vue'


// 修改：确保 inject 的键名与 provide 一致，并设置合理的默认值
const overViewStatus = inject<Ref<{ opcDaSub: boolean | null, tdEnginePub: boolean | null, mqttSub: boolean | null }>>(
  'overViewStatus',
  ref({ opcDaSub: null, tdEnginePub: null,mqttSub: null }) // 默认值结构保持一致
)

// 修改：监听 overViewStatus.value，确保深度监听生效
watch(
  () => overViewStatus?.value, // 监听 overViewStatus.value
  (newValue) => {
    console.log('OverView received overViewStatus:', newValue)
    // 确保 newValue 不为 null，否则使用默认值
    const validValue = newValue ?? { opcDaSub: null, tdEnginePub: null, mqttSub: null }
    overViewStatus.value = validValue // 更新本地状态
    console.log('overViewStatus value:', overViewStatus?.value?.opcDaSub, overViewStatus?.value?.tdEnginePub) // 修改：直接访问 opcDaSub 和 tdEnginePub
  },
  { deep: true } // 确保深度监听
)


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