<script setup lang="ts">
import { ref, provide, onMounted, onBeforeUnmount, computed } from "vue";
import OverView from "./components/OverView.vue";
import RtData from "./components/RtData.vue";
import TagsList from "./components/TagsList.vue";
import Configuration from "./components/Configuration.vue";
import LogView from "./components/LogView.vue";

// MQTT 全局连接
const mqttClient = ref<any>(null);
let checkTimer: any = null;

// 全局数据状态
const rtDataList = ref<
  {
    key: string;
    value: any;
    timestamp: string;
    quality: string;
    itemName: string;
  }[]
>([]);

// 新增：全局状态
const overViewStatus = ref<{
  opcDaSub: boolean | null;
  tdEnginePub: boolean | null;
}>({ opcDaSub: null, tdEnginePub: null });

// 全局日志
const appLog = ref<string[]>([]);

function log(msg: string) {
  const now = new Date();
  appLog.value.push(`[${now.toLocaleTimeString()}] ${msg}`);
  // 控制日志长度，避免无限增长
  if (appLog.value.length > 200) appLog.value.shift();
}

let messageRegistered = false;

function connectMqtt() {
  if (!window.mqtt) {
    log(
      "未检测到 MQTT 客户端库，请确认已在 index.html 中通过 CDN 引入 mqtt.min.js"
    );
    alert(
      "未检测到 MQTT 客户端库，请确认已在 index.html 中通过 CDN 引入 mqtt.min.js"
    );
    return;
  }
  if (mqttClient.value) {
    log("断开旧的 MQTT 连接");
    mqttClient.value.end(true);
    mqttClient.value = null;
    messageRegistered = false;
  }
  log("尝试连接 MQTT Broker...");
  mqttClient.value = window.mqtt.connect("ws://localhost:6882/mqtt", {
    reconnectPeriod: 5000,
    username: "admin",
    password: "123456",
  });

  mqttClient.value.on("connect", () => {
    log("MQTT 已连接");
    mqttClient.value.subscribe("/opcda/data");
    mqttClient.value.subscribe("/status/apps");
    log("已订阅 /opcda/data 和 /status/apps");
  });
  mqttClient.value.on("reconnect", () => {
    log("MQTT 正在重连...");
  });
  mqttClient.value.on("close", () => {
    log("MQTT 连接已关闭");
  });
  mqttClient.value.on("offline", () => {
    log("MQTT 已离线");
  });
  mqttClient.value.on("error", (err: any) => {
    log("MQTT 错误: " + (err?.message || err));
  });

  if (!messageRegistered) {
    mqttClient.value.on("message", (topic: string, message: Uint8Array) => {
      log(`收到消息 topic: ${topic}, payload: ${message.toString()}`);
      if (topic === "/opcda/data") {
        try {
          const json = JSON.parse(message.toString());
          for (const key of Object.keys(json)) {
            const idx = rtDataList.value.findIndex((row) => row.key === key);
            const newRow = {
              key,
              value: json[key]?.Value,
              timestamp: json[key]?.Timestamp,
              quality: json[key]?.Quality,
              itemName: json[key]?.ItemName,
            };
            if (idx !== -1) {
              rtDataList.value[idx] = newRow;
            } else {
              rtDataList.value.push(newRow);
            }
          }
        } catch (e) {
          log("解析 /opcda/data 消息失败: " + (e as Error).message);
        }
      } else if (topic === "/status/apps") {
        try {
          const data = JSON.parse(message.toString());
          overViewStatus.value.opcDaSub = data.opcDaSub;
          overViewStatus.value.tdEnginePub = data.tdEnginePub;
          // console.log('overViewStatus', overViewStatus.value)
        } catch (e) {
          log("解析 /status/apps 消息失败: " + (e as Error).message);
        }
      }
    });
    messageRegistered = true;
  }
}

onMounted(() => {
  connectMqtt();
  checkTimer = setInterval(() => {
    if (!mqttClient.value || !mqttClient.value.connected) {
      connectMqtt();
    }
  }, 10000);
});

onBeforeUnmount(() => {
  if (checkTimer) {
    clearInterval(checkTimer);
    checkTimer = null;
  }
  if (mqttClient.value) {
    mqttClient.value.end();
    mqttClient.value = null;
  }
});

// provide 全局状态
provide("mqttClient", mqttClient);
provide("rtDataList", rtDataList);
provide("overViewStatus", overViewStatus);
provide("appLog", appLog);

const tabs = [
  { name: "概览", key: "overview" },
  { name: "数据", key: "rtdata" },
  { name: "配置", key: "config" },
  { name: "点表", key: "tagslist" },
  { name: "日志", key: "logview" },
];
const activeTab = ref("overview");
const debugMode = ref(false); // 新增：Debug 模式开关

const paused = ref(false); // 新增：日志暂停状态
const displayedLog = computed(() => {
  return paused.value ? appLog.value.slice(0, appLog.value.length) : appLog.value;
});
</script>

<template>
  <div class="app-container">
    <header class="app-header">
      <div class="header-content">
        <span style="font-size: 2rem; font-weight: bold">应用头部</span>
        <!-- 新增：Debug 模式开关 -->
        <label style="margin-left: 20px">
          <input type="checkbox" v-model="debugMode" /> Debug 模式
        </label>
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
        <OverView v-if="activeTab === 'overview'" />
        <RtData v-else-if="activeTab === 'rtdata'" />
        <Configuration v-else-if="activeTab === 'config'" />
        <TagsList v-else-if="activeTab === 'tagslist'" />
        <LogView v-else-if="activeTab === 'logview'" msg="LogView" />
        <!-- 全局日志面板，受 debugMode 控制 -->

        <div v-if="debugMode">
            <div style="display: flex; align-items: center;">
            <div style="margin-left: 10px; font-weight: bold">[消息日志]</div>
            <button
              style="margin-left: 12px; padding: 2px 10px; font-size: 12px; cursor: pointer;"
              @click="appLog = []"
            >
              清空
            </button>
            </div>
          <div
            style="
              margin-top: 0px;
              max-height: 200px;
              overflow: auto;
              background: #f9f9f9;
              border: 1px solid #eee;
              padding: 8px;
              font-size: 12px;
            "
          >

            <div
              ref="logPanel"
              v-for="(line, idx) in displayedLog"
              :key="idx"
              style="white-space: pre-wrap; word-break: break-all;"
            >
              {{ line }}
            </div>
          </div>
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
