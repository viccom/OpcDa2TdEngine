<template>
  <div>
    <el-input v-model="filterText" placeholder="输入关键字过滤" style="margin-bottom: 10px; width: 300px;" />
    <div ref="tableWrapper" style="width: 99%;">
      <ElTableV2
        :columns="columns"
        :data="filteredData || []"
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

import { ref, computed, onMounted, onBeforeUnmount, nextTick, inject } from 'vue';
import { ElTableV2 } from 'element-plus';
import type { AnyColumn } from 'element-plus/es/components/table-v2/src/common';

// 通过 inject 获取全局 rtDataList，增加默认值
const rtDataList = inject<any>('rtDataList', ref([]));

// 过滤逻辑
const filterText = ref('');
const filteredData = computed(() =>
  (rtDataList.value ?? []).filter((item: any) =>
    item.key?.toLowerCase().includes(filterText.value.toLowerCase()) ||
    item.itemName?.toLowerCase().includes(filterText.value.toLowerCase())
  )
);

// el-table-v2 列定义
const columns: AnyColumn[] = [
  { key: 'key', dataKey: 'key', title: '点名', minWidth: 100, width: 120, flexGrow: 1 },
  { key: 'value', dataKey: 'value', title: '数值', minWidth: 100, width: 120, flexGrow: 1 },
  { key: 'timestamp', dataKey: 'timestamp', title: '时间戳', minWidth: 180, width: 200, flexGrow: 2 },
  { key: 'quality', dataKey: 'quality', title: '质量', minWidth: 80, width: 100, flexGrow: 1 },
  { key: 'itemName', dataKey: 'itemName', title: 'OPC_item', minWidth: 180, width: 200, flexGrow: 2 }
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

onMounted(() => {
  updateTableWidth();
  window.addEventListener('resize', updateTableWidth);
});

onBeforeUnmount(() => {
  window.removeEventListener('resize', updateTableWidth);
});
</script>