<template>
  <div>
    <div class="tagslist-toolbar" style="width: 99%;">
      <el-input v-model="filterText" placeholder="输入关键字过滤" style="width: 300px;" />
      <div class="tagslist-toolbar-btns">
        <el-button type="success" @click="clearRTdata">
          清空数据
        </el-button>
      </div>
    </div>
    <div ref="tableWrapper" style="width: 99%;">
      <ElTableV2
        :columns="columns"
        :data="sortedData || []"
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

// 排序相关逻辑
const sortBy = ref<any>('key'); // 修正类型为 any
const sortDirection = ref<'ASC' | 'DESC'>('ASC');

const handleSort = (params: { key: string; order: 'ASC' | 'DESC' }) => {
  sortBy.value = params.key;
  sortDirection.value = params.order;
};


const sortedData = computed(() => {
  const data = filteredData.value.slice();
  if (!sortBy.value) return data;
  return data.sort((a: any, b: any) => {
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

// el-table-v2 列定义，增加 sortable 属性
const columns: AnyColumn[] = [
  { key: 'key', dataKey: 'key', title: '点名', minWidth: 100, width: 120, flexGrow: 1, sortable: true },
  { key: 'value', dataKey: 'value', title: '数值', minWidth: 100, width: 120, flexGrow: 1, sortable: true },
  { key: 'timestamp', dataKey: 'timestamp', title: '时间戳', minWidth: 180, width: 200, flexGrow: 2, sortable: true },
  { key: 'quality', dataKey: 'quality', title: '质量', minWidth: 80, width: 100, flexGrow: 1, sortable: true },
  { key: 'itemName', dataKey: 'itemName', title: 'OPC_item', minWidth: 180, width: 200, flexGrow: 2, sortable: true }
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

// 新增 clearLog 方法
const clearRTdata = () => {
  rtDataList.value = []; // 清空日志数据
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