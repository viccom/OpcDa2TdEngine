<script setup lang="ts">
import { ref, provide, onMounted, onBeforeUnmount, computed, watch } from "vue";
import { nextTick } from "vue";
import OverView from "./components/OverView.vue";
import RtData from "./components/RtData.vue";
import TagsList from "./components/TagsList.vue";
import Configuration from "./components/Configuration.vue";
import LogView from "./components/LogView.vue";
import { ElSwitch, ElMessage } from "element-plus"; // 引入 ElMessage
import "element-plus/dist/index.css"; // 确保引入样式
import { WarnTriangleFilled } from '@element-plus/icons-vue'; // 引入 Search 图标


const inputMsg = ref<string>("");

const ServiceInstall = ref(false); // 新增：本地服务开关
const ServiceisRunning = ref(false); // 新增：本地服务启停按钮
// 新增：全局状态
const overViewStatus = ref<{
  opcDaSub: boolean | null;
  tdEnginePub: boolean | null;
}>({ opcDaSub: null, tdEnginePub: null });

// 全局日志
const appLog = ref<string[]>([]);

// 发送消息到 C#
const sendCSharp = () => {
  const message = JSON.stringify({
    type: inputMsg.value,
    reqid: "GUI-" + Math.random().toString(36).substr(2, 9),
  });
  if (window.external) {
    window.external.sendMessage(message);
  } else {
    console.error("Photino API 不可用");
  }
};


// 监听 C# 的消息
onMounted(() => {
  if (window.external) {
    window.external.receiveMessage((response) => {
      try {
        // 添加调试信息，确保 response 的原始值被记录
        console.log("原始响应:", response);

        // 确保 response 是对象类型
        let parsedResponse;
        if (typeof response === "string") {
          console.log("response为字符串:");
          parsedResponse = JSON.parse(response);
        } else {
          console.log("response为对象:");
          parsedResponse = response;
        }

        // 打印解析后的响应
        console.log("解析后的响应:", parsedResponse);

        // 检查 parsedResponse 是否包含 result 属性
        if (parsedResponse && parsedResponse.result !== undefined) {
          // 更新UI显示，增强对 payload 的处理
          const serviceStatus = parsedResponse.payload;
          const statusMessages = Object.entries(serviceStatus)
            .map(([serviceName, isRunning]) => `${serviceName}: ${isRunning ? "运行中" : "未运行"}`)
            .join(", ");
          inputMsg.value = `服务状态: ${statusMessages}`;
          //如果response正确解析，赋值给页面组件
          if (parsedResponse.type == "isInstall" && parsedResponse.result === true) {
              ServiceInstall.value = parsedResponse.result;
          } 
          if (parsedResponse.type == "isRun" && parsedResponse.result === false) {
            ServiceisRunning.value = parsedResponse.result;
          }
        } else {
          console.error("C# 返回错误:", parsedResponse?.result);
          console.log("服务状态查询结果:", JSON.stringify(parsedResponse?.payload));
          inputMsg.value = `错误: ${JSON.stringify(parsedResponse?.payload, null, 2)}`;
        }

      } 
      catch (e) 
      {
        console.error("解析C#消息失败:", e, "原始消息:", response);
        inputMsg.value = `非法响应格式: ${response}`;
      }
    });
  }
});

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

const logdata = ref<{
  ts: string;
  level: string;
  module: string;
  message: string;
}[]>([]);

function log(msg: string) {
  const now = new Date();
  appLog.value.push(`[${now.toLocaleTimeString()}] ${msg}`);
  // 控制日志长度，避免无限增长
  if (appLog.value.length > 200) appLog.value.shift();
}

let messageRegistered = false;

let clientid = "WEB_" + Math.random().toString(16).substr(2, 8);

const mqttHost = ref("127.0.0.1"); // 新增：节点IP输入框的绑定变量
const inputDisabled = ref(false); // 新增：输入框禁用状态

const isMqttConnected = ref(false); // 新增：MQTT连接状态

const manualConnect = ref(false); // 新增：手动连接MQTT的状态

const restartFlag = ref(false); // 新增：重启按钮的状态
const inRestart = ref(false); // 新增：重启状态
const logSubscribed = ref(false); // 记录日志订阅状态

const paused = ref(false); // 新增：日志暂停状态
const displayedLog = computed(() => {
  return paused.value ? appLog.value.slice(0, appLog.value.length) : appLog.value;
});

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
    isMqttConnected.value = false;
  }
  log("尝试连接 MQTT Broker...");

  mqttClient.value = window.mqtt.connect(`ws://${mqttHost.value || "localhost"}:6882/mqtt`, {
    reconnectPeriod: 5000,
    clientid,
    username: "admin",
    password: "123456",
  });

  mqttClient.value.on("connect", () => {
    log("MQTT 已连接");
    isMqttConnected.value = true;
    inputDisabled.value = true; // 连接成功后禁用输入框
    mqttClient.value.subscribe("/opcda/data");
    mqttClient.value.subscribe("/status/apps");
    log("已订阅 /opcda/data 和 /status/apps");
  });
  mqttClient.value.on("reconnect", () => {
    log("MQTT 正在重连...");
    isMqttConnected.value = false;
  });
  mqttClient.value.on("close", () => {
    log("MQTT 连接已关闭");
    overViewStatus.value.opcDaSub = null;
    overViewStatus.value.tdEnginePub = null; 
    isMqttConnected.value = false;
    inputDisabled.value = false; // 关闭时启用输入框
  });
  mqttClient.value.on("offline", () => {
    log("MQTT 已离线");
    overViewStatus.value.opcDaSub = null;
    overViewStatus.value.tdEnginePub = null; 
    isMqttConnected.value = false;
    inputDisabled.value = false; // 离线时启用输入框
  });
  mqttClient.value.on("error", (err: any) => {
    log("MQTT 错误: " + (err?.message || err));
    overViewStatus.value.opcDaSub = null;
    overViewStatus.value.tdEnginePub = null; 
    isMqttConnected.value = false;
    inputDisabled.value = false; // 连接失败时启用输入框
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
      else if (topic === '/logs/apps') {
        try {
          const log = JSON.parse(message.toString());
          logdata.value.push({
            ts: log.Timestamp,
            level: log.Level,
            module: log.Module,
            message: log.Message
          });
        } catch (e) {
          console.error('解析 MQTT 消息失败:', e);
        }
      }
    });
    messageRegistered = true;

  }
}

// 新增：断开MQTT连接
function disconnectMqtt() {
  if (mqttClient.value) {
    log("手动断开 MQTT 连接");
    mqttClient.value.end(true);
    mqttClient.value = null;
    isMqttConnected.value = false;
    messageRegistered = false;
    overViewStatus.value.opcDaSub = null;
    overViewStatus.value.tdEnginePub = null; 
    inputDisabled.value = false;
  }
}

// 新增：按钮点击逻辑
function handleMqttButton() {
  if (isMqttConnected.value) {
    disconnectMqtt();
    manualConnect.value = true;
  } else {
    connectMqtt();
    manualConnect.value = false;
  }
}
const handleLogSub = () => {
  if (!mqttClient || !mqttClient.value) {
    console.error('mqttClient 未连接，请检查连接状态！');
    return;
  }

  if (!logSubscribed.value) {
    try {
      // 订阅主题
      mqttClient.value.subscribe('/logs/apps', (err: any) => {
        if (err) {
          console.error('订阅失败:', err);
          return;
        }
        console.log('订阅成功');
        logSubscribed.value = true;
      });
    } catch (e) {
      console.error('订阅过程中发生异常:', e);
    }
  } else {
    try {
      // 取消订阅
      mqttClient.value.unsubscribe('/logs/apps', (err: any) => {
        if (err) {
          console.error('取消订阅失败:', err);
          return;
        }
        console.log('取消订阅成功');
        logSubscribed.value = false;
      });
      
    } catch (e) {
      console.error('取消订阅过程中发生异常:', e);
    }
  }
};

// 新增：重启按钮逻辑
function randomReqId() {
  return "web" + Math.floor(Math.random() * 1000000);
}

function handleRestart() {
  if (!mqttClient.value || !isMqttConnected.value) {
    ElMessage.warning("MQTT 未连接，无法发送重启命令");
    return;
  }
  const reqid = randomReqId();
  const topic = `client/${clientid}/command`;
  const payload = JSON.stringify({
    cmd: "set",
    type: "restart",
    data: "",
    reqid
  });
  // 订阅响应主题
  const respTopic = `client/${clientid}/response`;
  mqttClient.value.subscribe(respTopic, (err: any) => {
    if (err) {
      ElMessage.error("订阅响应主题失败: " + err.message);
    }
  });
  // 监听响应
  const onResponse = (topicResp: string, message: Uint8Array) => {
    if (topicResp === respTopic) {
      try {
        const resp = JSON.parse(message.toString());
        if (resp.reqid === reqid) {
          if (resp.result) {
            ElMessage.success(resp.message || "重启成功");
            restartFlag.value = false;
            inRestart.value = false;
            rtDataList.value = []; // 清空实时数据列表
          } else {
            ElMessage.error(resp.message || "重启失败");
          }
          // 只处理一次，移除监听
          mqttClient.value.off("message", onResponse);
        }
      } catch (e) {
        // 忽略解析错误
      }
    }
  };
  mqttClient.value.on("message", onResponse);
  // 发送命令
  mqttClient.value.publish(topic, payload);
  ElMessage.info("重启命令已发送");
  inRestart.value = true;
}

onMounted(() => {
  connectMqtt();
  checkTimer = setInterval(() => {
    if (!mqttClient.value || !mqttClient.value.connected ) {
      if (!manualConnect.value) {
      isMqttConnected.value = false;
      connectMqtt();
      }
    }
  }, 10000);

  // 挂到 window，供子组件读取
  (window as any).mqttClientId = clientid;
});

onBeforeUnmount(() => {
  if (checkTimer) {
    clearInterval(checkTimer);
    checkTimer = null;
  }
  if (mqttClient.value) {
    mqttClient.value.end();
    mqttClient.value = null;
    isMqttConnected.value = false;
  }
});

// provide 全局状态
provide("mqttClient", mqttClient);
provide("rtDataList", rtDataList);
provide("logdata", logdata);
provide('handleLogSub', handleLogSub);
provide("overViewStatus", overViewStatus);
provide("appLog", appLog);
provide('restartFlag', restartFlag);
provide('logSubscribed', logSubscribed);

const tabs = [
  { name: "概览", key: "overview" },
  { name: "数据", key: "rtdata" },
  { name: "配置", key: "config" },
  { name: "点表", key: "tagslist" },
  { name: "日志", key: "logview" },
];
const activeTab = ref("overview");
const configurationRef = ref();
const tagsListRef = ref();

const LOCAL_CONFIG_KEY = 'photinovue_config_cache';
const LOCAL_TAGS_KEY = 'photinovue_tags_cache';

const prevTab = ref(activeTab.value);

const debugMode = ref(false); // 新增：Debug 模式开关

// 测试 ServiceInstall 是否正常工作
const testServiceInstall = () => {
  console.log("ServiceInstall value:", ServiceInstall.value);
};

// 监听标签切换，处理本地缓存和自动加载
watch(
  activeTab,
  async (tab) => {
    console.log('[App.vue] 标签切换开始，当前标签:', tab, '上一个标签:', prevTab.value);

    // --- 离开 Configuration 时保存表单 ---
    if (prevTab.value === "config" && configurationRef.value && typeof configurationRef.value.getFormData === "function") {
      const configData = configurationRef.value.getFormData();
      if (configData && Object.values(configData).some(v => v && v !== '')) {
        sessionStorage.setItem(LOCAL_CONFIG_KEY, JSON.stringify(configData));
        console.log('[App.vue] Configuration 离开，已缓存表单数据');
      }
    }

    // --- 离开 TagsList 时保存表格 ---
    if (prevTab.value === "tagslist" && tagsListRef.value && typeof tagsListRef.value.getTableData === "function") {
      const tagsData = tagsListRef.value.getTableData();
      if (Array.isArray(tagsData) && tagsData.length > 0) {
        sessionStorage.setItem(LOCAL_TAGS_KEY, JSON.stringify(tagsData));
        console.log('[App.vue] TagsList 离开，已缓存表格数据');
      }
    }

    // 等待组件挂载完成
    await nextTick();

    // --- 进入 Configuration 时加载本地缓存 ---
    if (tab === "config" && configurationRef.value && typeof configurationRef.value.setFormData === "function") {
      const cache = sessionStorage.getItem(LOCAL_CONFIG_KEY);
      if (cache) {
        try {
          const parsed = JSON.parse(cache);
          configurationRef.value.setFormData(parsed);
          console.log('[App.vue] Configuration 激活，已加载本地缓存表单数据');
        } catch { }
      }
    }

    // --- 进入 TagsList 时加载本地缓存 ---
    if (tab === "tagslist" && tagsListRef.value && typeof tagsListRef.value.setTableData === "function") {
      const cache = sessionStorage.getItem(LOCAL_TAGS_KEY);
      if (cache) {
        try {
          const parsed = JSON.parse(cache);
          if (Array.isArray(parsed)) {
            tagsListRef.value.setTableData(parsed);
            console.log('[App.vue] TagsList 激活，已加载本地缓存表格数据');
          }
        } catch { }
      }
    }

    // 关键：切换后更新 prevTab
    console.log('[App.vue] 标签切换结束，当前标签:', tab, '上一个标签:', prevTab.value);
    prevTab.value = tab;
  }
);
</script>

<template>
  <div class="app-container">
    <header class="app-header">
      <div class="header-content" style="display: flex; align-items: flex-start; height: 60px;">
        <!-- 新增：水平排列的功能组件 -->
        <div style="display: flex; align-items: center; margin-left: 20px; height: 100%;">
          <label style="margin-right: 10px;">本地服务：</label>
          <el-switch 
            style="padding: 4px 12px; height: 24px; --el-switch-on-color: #13ce66; --el-switch-off-color: #ff4949" 
            v-model="ServiceInstall" 
            @change="testServiceInstall" 
            :active-text="ServiceInstall ? '已安装' : ''" 
            :inactive-text="ServiceInstall ? '' : '未安装'"
          /> <!-- 添加测试方法 -->
          <el-button
            style="padding: 4px 12px; border: none; border-radius: 4px; cursor: pointer; height: 24px;"
            :type="ServiceisRunning ? 'success' : 'info'"
          >
            {{ ServiceisRunning ? '已启动' : '已停止' }}
          </el-button>
        </div>
        <div style="display: flex; align-items: center; margin-left: 20px; height: 100%;">
          <label style="margin-right: 10px;">节点IP：</label>
            <input
            style="margin-right: 10px; padding: 4px; border: 1px solid #ccc; border-radius: 4px; height: 24px;"
            placeholder="请输入节点IP"
            v-model="mqttHost" 
            :disabled="inputDisabled"
            />
          <el-button
            :type="isMqttConnected ? 'danger' : 'primary'"
            style="padding: 4px 12px; border: none; border-radius: 4px; cursor: pointer; height: 24px;"
            @click="handleMqttButton"
          >
            {{ isMqttConnected ? '断开' : '连接' }}
          </el-button>

          <el-button
            type="warning"
            style="padding: 4px 12px; border: none; border-radius: 4px; cursor: pointer; height: 24px;"
            :icon="restartFlag ? WarnTriangleFilled : undefined"
            @click="handleRestart"
            :disabled="inRestart"
          >重启</el-button>
        </div>

        <div style="display: flex; align-items: center; margin-left: 20px; height: 100%;">
          <el-input v-model="inputMsg" 
                    placeholder="后端消息" 
                    style="margin-left: 20px; padding: 4px; border: 1px solid #ccc; border-radius: 4px; height: 24px;" 
          />
          <el-button
            type="primary"
            style="padding: 4px 12px; border: none; border-radius: 4px; cursor: pointer; height: 24px;"
            @click="sendCSharp"
          >触发</el-button>
        </div>

        <!-- 新增：Debug 模式开关 -->
        <div style="display: flex; align-items: center; margin-left: 20px; height: 100%;">
          <label style="margin-left: 20px">
            <input type="checkbox" v-model="debugMode" /> MQTT消息
          </label>
        </div>
      </div>
    </header>
    <main class="app-main">
      <div class="tabs">
        <div v-for="tab in tabs" :key="tab.key" :class="['tab', { active: activeTab === tab.key }]"
          @click="activeTab = tab.key">
          {{ tab.name }}
        </div>
      </div>
      <div class="tab-content">
        <OverView v-if="activeTab === 'overview'" />
        <RtData v-else-if="activeTab === 'rtdata'" />
        <Configuration v-else-if="activeTab === 'config'" ref="configurationRef" />
        <TagsList v-else-if="activeTab === 'tagslist'" ref="tagsListRef" />
        <LogView v-else-if="activeTab === 'logview'" msg="LogView" />
        <!-- 全局日志面板，受 debugMode 控制 -->

        <div v-if="debugMode">
          <div style="display: flex; align-items: center;">
            <div style="margin-left: 10px; font-weight: bold">[消息日志]</div>
            <button style="margin-left: 12px; padding: 2px 10px; font-size: 12px; cursor: pointer;"
              @click="appLog = []">
              清空
            </button>
          </div>
          <div style="
              margin-top: 0px;
              max-height: 200px;
              overflow: auto;
              background: #f9f9f9;
              border: 1px solid #eee;
              padding: 8px;
              font-size: 12px;
            ">

            <div ref="logPanel" v-for="(line, idx) in displayedLog" :key="idx"
              style="white-space: pre-wrap; word-break: break-all;">
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
  justify-content: flex-start; /* 修改为靠左对齐 */
  border-bottom: 1px solid #e0e0e0;
}

.header-content {
  display: flex;
  align-items: center;
  height: 60px; /* 统一高度 */
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
