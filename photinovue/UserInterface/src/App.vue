<script setup lang="ts">
import { ref } from 'vue'
import OverView from './components/OverView.vue'
import RtData from './components/RtData.vue'
import TagsList from './components/TagsList.vue'
import Configuration from './components/Configuration.vue'
import LogView from './components/LogView.vue'

const tabs = [
  { name: '概览', key: 'overview' },
  { name: '数据', key: 'rtdata' },
  { name: '配置', key: 'config' },
  { name: '点表', key: 'tagslist' },
  { name: '日志', key: 'logview' }
]
const activeTab = ref('overview')
</script>

<template>
  <div class="app-container">
    <header class="app-header">
      <div class="header-content">
        <!-- 头部内容，后续可自定义 -->
        <span style="font-size: 2rem; font-weight: bold;">应用头部</span>
      </div>
    </header>
    <main class="app-main">
      <div class="tabs">
        <div
          v-for="tab in tabs"
          :key="tab.key"
          :class="['tab', { active: activeTab === tab.key }]"
          @click="activeTab = tab.key"
        >
          {{ tab.name }}
        </div>
      </div>
      <div class="tab-content">
        <div v-if="activeTab === 'overview'">
          <OverView />
        </div>
        <div v-else-if="activeTab === 'rtdata'">
          <!-- 数据内容 -->
          <RtData />
        </div>
        <div v-else-if="activeTab === 'config'">
          <!-- 配置内容 -->
          <Configuration/>
        </div>
        <div v-else-if="activeTab === 'tagslist'">
          <!-- 点表内容 -->
          <TagsList />
        </div>
        <div v-else-if="activeTab === 'logview'">
          <!-- 日志内容 -->
          <LogView msg="LogView" />
        </div>
      </div>
    </main>
  </div>
</template>

<style scoped>
.app-container {
  width: 98vw;
  height: 99vh;
  display: flex;
  flex-direction: column;
}
.app-header {
  height: 80px;
  background: #f5f7fa;
  display: flex;
  align-items: center;
  justify-content: center;
  border-bottom: 1px solid #e0e0e0;
}
.header-content {
  width: 100%;
  text-align: center;
}
.app-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: #fff;
  overflow: hidden;
}
.tabs {
  display: flex;
  border-bottom: 2px solid #e0e0e0;
  background: #fafbfc;
}
.tab {
  width: 120px;
  text-align: center;
  padding: 16px 0;
  cursor: pointer;
  font-weight: 500;
  color: #666;
  border-right: 1px solid #e0e0e0;
  transition: background 0.2s, color 0.2s;
  position: relative;
}
.tab:last-child {
  border-right: none;
}
.tab.active {
  color: #42b983;
  background: #fff;
  border-bottom: 2px solid #42b983;
  font-weight: bold;
}
.tab-content {
  flex: 1;
  padding: 24px;
  overflow: auto;
}
</style>
