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
import { ref, computed, onMounted, nextTick } from 'vue';
import { ElTableV2 } from 'element-plus';
import type { AnyColumn } from 'element-plus/es/components/table-v2/src/common';

// 模拟数据
const data = ref(
  Array.from({ length: 10000 }, (_, i) => ({
    id: i + 1,
    name: `Item ${i + 1}`,
    value: Math.floor(Math.random() * 100),
  }))
);

// 过滤逻辑
const filterText = ref('');
const filteredData = computed(() =>
  data.value.filter((item) =>
    item.name.toLowerCase().includes(filterText.value.toLowerCase())
  )
);

// el-table-v2 列定义
const columns: AnyColumn[] = [
  { key: 'id', dataKey: 'id', title: 'ID', minWidth: 80, width: 80, flexGrow: 1 },
  { key: 'name', dataKey: 'name', title: '名称', minWidth: 200, width: 200, flexGrow: 3 },
  { key: 'value', dataKey: 'value', title: '值', minWidth: 80, width: 80, flexGrow: 1 }
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
</script>