using System;
using System.Data;
using System.IO;
using System.ServiceProcess;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Threading;
using System.Timers;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace OPCDAClient_GUI
{


    public partial class Form1 : Form
    {
        // 新增：在Form1中添加对服务状态检查的支持
        private Thread _checkStatusThread;
        private System.Timers.Timer _statusTimer;
        private HttpClient _httpClient;
        // 新增：MQTTClient
        private IMqttClient _mqttClient;
        private IMqttClientOptions _mqttOptions;
        private readonly string _mqClientId = "GUI_" + GenerateShortGuid();
        
        public Form1()
        {
            InitializeComponent();
            InitializeInstallChecker();
            // 新增：绑定 mqtt_bt 按钮点击事件
            mqtt_bt.Click += Mqtt_bt_Click;
        }

        private void InitializeInstallChecker()
        {
            _checkStatusThread = new Thread(new ThreadStart(CheckServiceStatus));
            _checkStatusThread.IsBackground = true;
            _checkStatusThread.Start();
        }
        // 新增：周期性检查服务状态的方法
        private void CheckServiceStatus()
        {
            while (true)
            {
                bool exists = CheckServiceExists("OpcDa2TdEngine");
                // 使用Invoke更新UI
                this.Invoke((MethodInvoker)delegate
                {
                    UpdateCheckBox(exists);
                });
                // 检查服务是否运行
                if (exists)
                {
                    bool Service_Run = CheckServiceRun("OpcDa2TdEngine");
                    // 使用Invoke更新UI
                    this.Invoke((MethodInvoker)delegate
                    {
                        UpdateInstallBt(Service_Run);
                    });
                }
                Thread.Sleep(3000); // 每5秒检查一次
            }
        }

        // 新增：检查服务是否存在
        private bool CheckServiceExists(string serviceName)
        {
            foreach (ServiceController service in ServiceController.GetServices())
            {
                if (service.ServiceName == serviceName)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckServiceRun(string serviceName)
        {
            foreach (ServiceController service in ServiceController.GetServices())
            {
                if (service.ServiceName == serviceName)
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // 新增：更新CheckBox状态的方法
        private void UpdateCheckBox(bool exists)
        {
            if (exists)
            {
                Inst_install.Text = "已安装";
                Inst_install.BackColor = System.Drawing.Color.SeaGreen;
            }
            else
            {
                Inst_install.Text = "未安装";
                Inst_install.BackColor = System.Drawing.Color.OrangeRed;
            }
        }

        private void UpdateInstallBt(bool exists)
        {
            if (exists)
            {
                install_bt.Text = "停止";
                install_bt.BackColor = System.Drawing.Color.OrangeRed;
            }
            else
            {
                install_bt.Text = "启动";
                install_bt.BackColor = System.Drawing.Color.Gray;
            }
        }

        private void InitializeStatusChecker()
        {
            _httpClient = new HttpClient();
            _statusTimer = new System.Timers.Timer(3000);
            _statusTimer.Elapsed += StatusTimer_Elapsed;
            _statusTimer.AutoReset = true;
            _statusTimer.Enabled = true;
            _statusTimer.Start();
        }

        // 修改点：将事件处理方法改为async void以支持await
        private async void StatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await CheckServiceStatusAsync(); // 现在可以正确await异步方法
        }

        private async Task CheckServiceStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("http://127.0.0.1:7890/apiv1/service_Status");

                string logMessage = $"Request to /apiv1/service_Status returned status code: {response.StatusCode}";

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<ServiceStatusResponse>(content);
                    UpdateControls(result);
                    logMessage += $", Response Body: {content}";
                }
                else
                {
                    // 当网络不可达或请求失败时，模拟返回固定数据
                    var fallbackResult = new ServiceStatusResponse
                    {
                        result = false,
                        data = new ServiceData { opcDaSub = false, tdEnginePub = false },
                        message = "bad"
                    };
                    UpdateControls(fallbackResult);
                    logMessage += ", Request failed.";
                }

                AppendToLogs(logMessage);
            }
            catch (Exception ex)
            {
                // 当发生异常时，模拟返回固定数据
                var fallbackResult = new ServiceStatusResponse
                {
                    result = false,
                    data = new ServiceData { opcDaSub = false, tdEnginePub = false },
                    message = "bad"
                };
                UpdateControls(fallbackResult);

                string errorMessage = $"Error occurred while checking service status: {ex.Message}";
                AppendToLogs(errorMessage);
            }
        }

        private void UpdateControls(ServiceStatusResponse data)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateControls(data)));
                return;
            }

            // 更新后台服务安装状态
            if (data.result)
            {
                Inst_run.Text = "已启动";
                Inst_run.BackColor = Color.SeaGreen;
            }
            else if (!data.result && data.message == "bad")
            {
                Inst_run.Text = "异常";
                Inst_run.BackColor = Color.Red;
            }
            else
            {
                Inst_run.Text = "未启动";
                Inst_run.BackColor = Color.OrangeRed;
            }

            // 更新OPC DA服务状态
            UpdateLabel(opcda_run, data.data.opcDaSub);
            UpdateButton(opcda_bt, data.data.opcDaSub);

            // 更新TDengine服务状态
            UpdateLabel(tdengine_run, data.data.tdEnginePub);
            UpdateButton(tdengine_bt, data.data.tdEnginePub);
        }

        
        private void MqUpdateControls(ServiceData data)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => MqUpdateControls(data)));
                return;
            }
            opcda_bt.Visible = true;
            tdengine_bt.Visible = true;
            
            restart.Visible = true;
            Inst_run.Text = "已启动";
            Inst_run.BackColor = Color.SeaGreen;
        
            // 更新OPC DA服务状态
            UpdateLabel(opcda_run, data.opcDaSub);
            UpdateButton(opcda_bt, data.opcDaSub);

            // 更新TDengine服务状态
            UpdateLabel(tdengine_run, data.tdEnginePub);
            UpdateButton(tdengine_bt, data.tdEnginePub);


        }
        private void UpdateLabel(Label lb, bool isRunning)
        {
            lb.Text = isRunning ? "运行中" : "已停止";
            lb.BackColor = isRunning ? Color.SeaGreen : Color.OrangeRed;
        }
        private void UpdateButton(Button btn, bool isRunning)
        {
            btn.Text = isRunning ? "停止" : "启动";
            btn.BackColor = isRunning ? Color.DarkOrange : Color.SeaGreen;
        }

        private void AppendToLogs(string logMessage)
        {
            if (logsText.InvokeRequired)
            {
                logsText.Invoke(new Action(() => AppendToLogs(logMessage)));
                return;
            }

            // 将日志信息追加到TextBox中
            logsText.AppendText($"{DateTime.Now}: {logMessage}{Environment.NewLine}");
        }

        private void Upload_bt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.Title = "Select a CSV file";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string[] csvLines = File.ReadAllLines(openFileDialog.FileName);

                        // Clear existing rows in the DataGridView
                        tagsTable.Rows.Clear();

                        // Skip the first line (header) and parse the remaining lines
                        for (int i = 1; i < csvLines.Length; i++)
                        {
                            string line = csvLines[i];
                            string[] values = line.Split(',');

                            // Skip rows with empty data
                            if (values.Length == 4 && !string.IsNullOrWhiteSpace(values[0]) &&
                                !string.IsNullOrWhiteSpace(values[1]) &&
                                !string.IsNullOrWhiteSpace(values[2]) &&
                                !string.IsNullOrWhiteSpace(values[3]))
                            {
                                // Add values to the DataGridView in the order of predefined headers
                                tagsTable.Rows.Add(values[0], values[1], values[2], values[3]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reading CSV file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void opcda_bt_Click(object sender, EventArgs e)
        {
            await AppControls(opcda_bt);
        }

        private async void tdengine_bt_Click(object sender, EventArgs e)
        {
            await AppControls(tdengine_bt);
        }


        private async Task AppControls(Button btn)
        {
            try
            {
                string id = string.Empty;
                if (btn == opcda_bt)
                {
                    id = "opcDaSub";
                }
                else if (btn == tdengine_bt)
                {
                    id = "tdEnginePub";
                }
                else
                {
                    return; // 如果不是预期按钮，则退出
                }

                string jsonPayload = $"{{\"id\": \"{id}\"}}";
                string url = string.Empty;

                // 判断按钮的当前Text，决定调用哪个接口
                if (btn.Text == "停止")
                {
                    url = "http://127.0.0.1:7890/apiv1/stop";
                }
                else if (btn.Text == "启动")
                {
                    url = "http://127.0.0.1:7890/apiv1/start";
                }
                else
                {
                    return;
                }

                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending request: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void restart_Click(object sender, EventArgs e)
        {
            if (Inst_run.Text == "已启动")
            {
                if (MessageBox.Show("确定要重启服务吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    await RestartService("opcDaSub");
                    await RestartService("tdEnginePub");
                }
            }
            // 同时重启两个服务
            //await Task.WhenAll(RestartService("opcDaSub"), RestartService("tdEnginePub"));
        }


        private async Task RestartService(string id)
        {
            try
            {
                string jsonPayload = $"{{\"id\": \"{id}\"}}";
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

                // 停止服务
                var stopResponse = await _httpClient.PostAsync("http://127.0.0.1:7890/apiv1/stop", content);
                stopResponse.EnsureSuccessStatusCode();

                // 可选：等待500毫秒确保服务停止生效
                await Task.Delay(1500);

                // 启动服务
                var startResponse = await _httpClient.PostAsync("http://127.0.0.1:7890/apiv1/start", content);
                startResponse.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restarting service {id}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void install_bt_Click(object sender, EventArgs e)
        {
            if (Inst_install.Text != "已安装")
            {
                MessageBox.Show("本地服务未安装", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (install_bt.Text == "启动")
            {
                if (MessageBox.Show("确定要启动服务吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    StartService("OpcDa2TdEngine");
                }
            }
            else
            {
                if (MessageBox.Show("确定要停止服务吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    StopService("OpcDa2TdEngine");
                }
            }
        }
        private void StartService(string serviceName)
        {
            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status != ServiceControllerStatus.Running)
                {
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                    AppendToLogs($"服务 {serviceName} 启动成功.");
                }
                else
                {
                    AppendToLogs($"服务 {serviceName} 已经处于运行状态.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动服务 {serviceName} 时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopService(string serviceName)
        {
            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                    AppendToLogs($"服务 {serviceName} 已停止.");
                }
                else
                {
                    AppendToLogs($"服务 {serviceName} 当前未在运行.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"停止服务 {serviceName} 时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 新增：MQTT 连接函数
        private async Task ConnectMqttAsync()
        {
            try
            {
                var mqttFactory = new MqttFactory();
                _mqttClient = mqttFactory.CreateMqttClient();

                var brokerIp = mqtt_node.Text.Trim();
                if (string.IsNullOrEmpty(brokerIp))
                {
                    MessageBox.Show("请输入有效的 MQTT Broker IP 地址", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _mqttOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(brokerIp, 6883)
                    .WithClientId(_mqClientId)
                    .WithCredentials("admin", "123456")
                    .Build();

                var response = await _mqttClient.ConnectAsync(_mqttOptions, CancellationToken.None);

                if (response.ResultCode.ToString() == "Success") // 替代 MqttClientConnectResultCode.Success
                {
                    AppendToLogs("MQTT 连接成功");
                    mqtt_node.Enabled = false;
                    mqtt_bt.Invoke((MethodInvoker)delegate { mqtt_bt.Text = "断开"; });

                    // 订阅多个主题 /status/apps
                    // var topicFilter = new MqttTopicFilterBuilder()
                    //     .WithTopic("/status/apps")
                    //     .Build();
                    await _mqttClient.SubscribeAsync("/status/apps");
                    await _mqttClient.SubscribeAsync("/opcda/data");
                    await _mqttClient.SubscribeAsync("client/" + _mqClientId + "/response");

                    AppendToLogs("已订阅主题: /status/apps");

                    // 设置消息接收处理
                    _mqttClient.UseApplicationMessageReceivedHandler(e =>
                    {
                        var topic = e.ApplicationMessage.Topic;
                        var payload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                        
                        if (topic == "client/" + _mqClientId + "/response")
                        {
                            // 处理 /client/<clientId>/response 主题的消息
                            // ...
                        }
                        if (topic == "/status/apps")
                        {
                            AppendToLogs($"收到消息: {payload}");
                            try
                            {
                                var status = JsonConvert.DeserializeObject<ServiceData>(payload);
                                this.Invoke((MethodInvoker)delegate
                                {
                                    MqUpdateControls(status);
                                });
                            }
                            catch (Exception ex)
                            {
                                AppendToLogs($"解析 MQTT 消息失败: {ex.Message}");
                            }
                        }
                        if (topic == "/opcda/data")
                        {
                            // 处理 /opcda/data 主题的消息
                        }

                        
                    });
                }
                else
                {
                    AppendToLogs($"MQTT 连接失败: {response.ResultCode}");
                }
            }
            catch (Exception ex)
            {
                AppendToLogs($"MQTT 连接异常: {ex.Message}");
            }
        }

        // 新增：MQTT 断开连接函数
        private async Task DisconnectMqttAsync()
        {
            try
            {
                if (_mqttClient != null && _mqttClient.IsConnected)
                {
                    await _mqttClient.DisconnectAsync();
                    AppendToLogs("MQTT 已断开连接");
                    mqtt_bt.Invoke((MethodInvoker)delegate { mqtt_bt.Text = "连接"; });
                    mqtt_node.Enabled = true;
                    Inst_run.Text = "";
                    Inst_run.BackColor = Color.Gray;
                    restart.Visible = false;
                    opcda_run.Text = "";
                    opcda_bt.Visible = false;
                    tdengine_run.Text = "";
                    tdengine_bt.Visible = false;
                    
                }
            }
            catch (Exception ex)
            {
                AppendToLogs($"MQTT 断开连接异常: {ex.Message}");
            }
        }

        // 新增：mqtt_bt 按钮点击事件处理
        private async void Mqtt_bt_Click(object sender, EventArgs e)
        {
            if (_mqttClient == null || !_mqttClient.IsConnected)
            {
                await ConnectMqttAsync();
            }
            else
            {
                await DisconnectMqttAsync();
            }
        }

        
        public static string GenerateShortGuid(int length = 8)
        {
            return Guid.NewGuid().ToString("N").Substring(0, length); // 取前8位
        }

    }

    // 新增数据模型类
    public class ServiceStatusResponse
    {
        public bool result { get; set; }
        public ServiceData data { get; set; }
        public string message { get; set; }
    }

    public class ServiceData
    {
        public bool opcDaSub { get; set; }
        public bool tdEnginePub { get; set; }
    }

}