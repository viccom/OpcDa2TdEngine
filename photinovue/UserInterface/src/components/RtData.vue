<template>
  <div>
    <el-input v-model="filterText" placeholder="输入关键字过滤" style="margin-bottom: 10px; width: 300px;" />
    <div ref="tableWrapper" style="width: 99%;">
      <ElTableV2
        :columns="columns"
        :data="filteredData"
        :width="tableWidth"
        :height="500"
        style="border: 1px solid #ebeef5;"
      />
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

import { ref, computed, onMounted, onBeforeUnmount, nextTick } from 'vue';
import { ElTableV2 } from 'element-plus';
import type { AnyColumn } from 'element-plus/es/components/table-v2/src/common';

// mqtt 相关
let client: any = null;
let checkTimer: any = null;

// mqtt 数据
const data = ref<{ key: string, value: any, timestamp: string, quality: string, itemName: string }[]>([]);

// 过滤逻辑
const filterText = ref('');
const filteredData = computed(() =>
  data.value.filter((item) =>
    item.key.toLowerCase().includes(filterText.value.toLowerCase()) ||
    item.itemName.toLowerCase().includes(filterText.value.toLowerCase())
  )
);

// el-table-v2 列定义
const columns: AnyColumn[] = [
  { key: 'key', dataKey: 'key', title: 'Key', minWidth: 100, width: 120, flexGrow: 1 },
  { key: 'value', dataKey: 'value', title: 'Value', minWidth: 100, width: 120, flexGrow: 1 },
  { key: 'timestamp', dataKey: 'timestamp', title: 'Timestamp', minWidth: 180, width: 200, flexGrow: 2 },
  { key: 'quality', dataKey: 'quality', title: 'Quality', minWidth: 80, width: 100, flexGrow: 1 },
  { key: 'itemName', dataKey: 'itemName', title: 'ItemName', minWidth: 180, width: 200, flexGrow: 2 }
];

// 动态获取容器宽度，传递给 :width
const tableWidth = ref(800);
const tableWrapper = ref<HTMLElement | null>(null);

const updateTableWidth = () => {
  nextTick(() => {
    if (tableWrapper.value) {
      tableWidth.value = tableWrapper.value.offsetWidth;
    }
  });
};

// mqtt 连接与重连
function connectMqtt() {
  if (!window.mqtt) {
    alert('未检测到 MQTT 客户端库，请确认已在 index.html 中通过 CDN 引入 mqtt.min.js');
    return;
  }
  if (client) {
    client.end(true);
    client = null;
  }
  client = window.mqtt.connect('ws://localhost:6882/mqtt', {
    reconnectPeriod: 5000,
    username: 'admin',
    password: '123456'
  });
  client.on('connect', () => {
    client.subscribe('/opcda/data');
  });
  client.on('reconnect', () => {
    // 可选：记录日志
  });
  client.on('close', () => {
    // 可选：记录日志
  });
  client.on('offline', () => {
    // 可选：记录日志
  });
  client.on('error', () => {
    // 可选：记录日志
  });
  client.on('message', (topic: string, message: Uint8Array) => {
    if (topic === '/opcda/data') {
      try {
        const json = JSON.parse(message.toString());
        // 增量更新表格数据
        for (const key of Object.keys(json)) {
          const idx = data.value.findIndex(row => row.key === key);
          const newRow = {
            key,
            value: json[key]?.Value,
            timestamp: json[key]?.Timestamp,
            quality: json[key]?.Quality,
            itemName: json[key]?.ItemName
          };
          if (idx !== -1) {
            // 存在则更新
            data.value[idx] = newRow;
          } else {
            // 不存在则新增
            data.value.push(newRow);
          }
        }
      } catch (e) {
        // 可选：记录日志
      }
    }
  });
}

onMounted(() => {
  updateTableWidth();
  window.addEventListener('resize', updateTableWidth);
  connectMqtt();
  checkTimer = setInterval(() => {
    if (!client || !client.connected) {
      connectMqtt();
    }
  }, 10000);
});

onBeforeUnmount(() => {
  window.removeEventListener('resize', updateTableWidth);
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