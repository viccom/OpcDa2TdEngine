using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Threading;
using System.Timers;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json.Linq;
using Zuby.ADGV;

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
        private System.Timers.Timer _mqttKeepAliveTimer;

        private readonly Size _designSize = new Size(1600, 900); // 设计时的原始尺寸
        private float _dpiScale = 1f;

        public Form1()
        {
            // 必须在 InitializeComponent 前设置
            this.AutoScaleMode = AutoScaleMode.Dpi; // 自动缩放模式
            this.AutoSize = false; // 禁用自动调整大小
            this.Size = _designSize; // 初始化为设计尺寸

            InitializeComponent();

            // 设置字体避免模糊
            this.Font = new Font("Microsoft YaHei", 9F, FontStyle.Regular, GraphicsUnit.Point);

            // 禁用自动缩放（使用手动DPI缩放）
            this.AutoSize = false;
            this.AutoSizeMode = AutoSizeMode.GrowOnly;

            InitializeInstallChecker();
            // 新增：绑定 mqtt_bt 按钮点击事件
            mqtt_bt.Click += Mqtt_bt_Click;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ScaleWindow(); // 首次加载时缩放
        }
        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            _dpiScale = e.DeviceDpiNew / 96f;
            ScaleWindow(); // DPI变化时重新缩放
        }
        private void ScaleWindow()
        {
            // 缩放窗口尺寸
            this.Size = new Size(
                (int)(_designSize.Width * _dpiScale),
                (int)(_designSize.Height * _dpiScale)
            );
            // 缩放所有子控件
            foreach (Control control in this.Controls)
            {
                ScaleControl(control, _dpiScale);
            }
        }
        private void ScaleControl(Control control, float scaleFactor)
        {
            // 缩放位置和尺寸
            control.Left = (int)(control.Left * scaleFactor);
            control.Top = (int)(control.Top * scaleFactor);
            control.Width = (int)(control.Width * scaleFactor);
            control.Height = (int)(control.Height * scaleFactor);
            // 缩放字体（可选，根据需求）
            //control.Font = new Font(control.Font.FontFamily,
            //                      control.Font.SizeInPoints * scaleFactor,
            //                      control.Font.Style);
            // 特殊控件处理（如DataGridView）
            if (control is DataGridView dgv)
            {
                foreach (DataGridViewColumn column in dgv.Columns)
                {
                    column.Width = (int)(column.Width * scaleFactor);
                }
                dgv.RowTemplate.Height = (int)(dgv.RowTemplate.Height * scaleFactor);
            }
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

            restart.Visible = false;
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
            //logsText.ScrollToCaret(); // 滚动到文本框的最底部
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
                string mqPayload = string.Empty;
                string reqid = GenerateShortGuid(8); // 生成唯一的请求ID

                // 判断按钮的当前Text，决定调用哪个接口
                if (btn.Text == "停止")
                {
                    url = "http://127.0.0.1:7890/apiv1/stop";
                    mqPayload = $"{{\"type\": \"stop\", \"cmd\": \"set\", \"data\": \"{id}\", \"reqid\": \"{reqid}\"}}";
                }
                else if (btn.Text == "启动")
                {
                    url = "http://127.0.0.1:7890/apiv1/start";
                    mqPayload = $"{{\"type\": \"start\", \"cmd\": \"set\", \"data\": \"{id}\", \"reqid\": \"{reqid}\"}}";
                }
                else
                {
                    return;
                }

                await _mqttClient.PublishAsync("client/" + _mqClientId + "/command", mqPayload, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false);
                string cmdMessage = $"发送主题：{"client/" + _mqClientId + "/command"}，指令: {mqPayload}";
                AppendToLogs(cmdMessage);
                //var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                //var response = await _httpClient.PostAsync(url, content);
                //response.EnsureSuccessStatusCode();
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
                    install_bt.Text = "启动";
                    install_bt.BackColor = Color.SeaGreen;
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
                            AppendToLogs($"收到消息: {payload}");
                            UpdateAdvancedDataGridViewWithMqttData(payload, rtDataGridView);
                        }


                    });

                    // 新增：启动 MQTT 连接保持定时器
                    _mqttKeepAliveTimer = new System.Timers.Timer(3000); // 每10秒检查一次
                    _mqttKeepAliveTimer.Elapsed += MqttKeepAliveTimer_Elapsed;
                    _mqttKeepAliveTimer.AutoReset = true;
                    _mqttKeepAliveTimer.Enabled = true;
                    _mqttKeepAliveTimer.Start();
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

        public static void UpdateAdvancedDataGridViewWithMqttData(string jsonData, AdvancedDataGridView dataGridView)
        {
            // 解析JSON数据
            var jsonObject = JObject.Parse(jsonData);

            // 确保在UI线程操作
            if (dataGridView.InvokeRequired)
            {
                dataGridView.Invoke(new MethodInvoker(() =>
                    UpdateAdvancedDataGridViewWithMqttData(jsonData, dataGridView)));
                return;
            }
            // 锁定UI更新
            dataGridView.SuspendLayout();

            try
            {
                // 初始化数据源（如果是第一次）
                if (dataGridView.DataSource == null)
                {
                    var table = new DataTable();

                    // 创建列并设置初始宽度
                    table.Columns.Add("点名", typeof(string));    // Column1
                    table.Columns.Add("数值", typeof(object));   // Column2
                    table.Columns.Add("时间", typeof(DateTime)); // Column3
                    table.Columns.Add("质量", typeof(string));   // Column4
                    table.Columns.Add("OPC_item", typeof(string)); // Column5

                    dataGridView.DataSource = table;

                    // 设置固定列宽（单位：像素）
                    dataGridView.Columns["点名"].Width = 250;    // 点名列稍宽
                    dataGridView.Columns["数值"].Width = 300;
                    dataGridView.Columns["时间"].Width = 360;    // 时间列需要更多空间
                    dataGridView.Columns["质量"].Width = 150;
                    dataGridView.Columns["OPC_item"].Width = 400;

                    // 禁止用户调整列宽
                    foreach (DataGridViewColumn column in dataGridView.Columns)
                    {
                        column.Resizable = DataGridViewTriState.False;
                    }

                    // 设置时间列格式
                    dataGridView.Columns["时间"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
                }
                var dataTable = (DataTable)dataGridView.DataSource;
                foreach (var item in jsonObject)
                {
                    string pointName = item.Key;
                    var values = (JObject)item.Value;
                    // 查找是否已存在该点
                    DataRow[] existingRows = dataTable.Select($"[点名] = '{pointName.Replace("'", "''")}'");
                    if (existingRows.Length > 0)
                    {
                        // 更新现有行
                        DataRow row = existingRows[0];
                        row["数值"] = values["Value"].ToObject<object>();
                        row["时间"] = values["Timestamp"].ToObject<DateTime>();
                        row["质量"] = values["Quality"].ToString();
                        row["OPC_item"] = values["ItemName"].ToString();
                    }
                    else
                    {
                        // 添加新行
                        DataRow newRow = dataTable.NewRow();
                        newRow["点名"] = pointName;
                        newRow["数值"] = values["Value"].ToObject<object>();
                        newRow["时间"] = values["Timestamp"].ToObject<DateTime>();
                        newRow["质量"] = values["Quality"].ToString();
                        newRow["OPC_item"] = values["ItemName"].ToString();
                        dataTable.Rows.Add(newRow);
                    }
                }
                // 刷新绑定（通知界面更新）
                dataTable.AcceptChanges();
            }
            catch (Exception ex)
            {
                // 建议添加日志记录
                Console.WriteLine($"更新表格时出错: {ex.Message}");
            }
            finally
            {
                dataGridView.ResumeLayout();

                // 特别注意：不再调用AutoResizeColumns以保持固定宽度
            }
        }
        // 新增：MQTT 连接保持定时器事件处理
        private async void MqttKeepAliveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_mqttClient == null || !_mqttClient.IsConnected)
            {
                _mqttKeepAliveTimer.Stop();
                AppendToLogs("检测到 MQTT 连接已断开");
                mqtt_bt.Invoke((MethodInvoker)delegate { mqtt_bt.Text = "连接"; });
                mqtt_node.Enabled = true;
                Inst_run.Text = "";
                Inst_run.BackColor = Color.Gray;
                restart.Visible = false;
                opcda_run.Text = "";
                opcda_bt.Visible = false;
                tdengine_run.Text = "";
                tdengine_bt.Visible = false;
                // 提示用户连接已断开
                // MessageBox.Show("MQTT 连接已断开，请检查网络或重新连接", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // 尝试重新连接
                // await ConnectMqttAsync();
            }
        }

        // 新增：断开连接时停止定时器
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

                    // 停止定时器
                    if (_mqttKeepAliveTimer != null)
                    {
                        _mqttKeepAliveTimer.Stop();
                        _mqttKeepAliveTimer.Dispose();
                    }
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

        //
        private async void Inst_install_Click(object sender, EventArgs e)
        {
            if (Inst_install.Text == "已安装")
            {
                if (install_bt.Text == "停止")
                {
                    MessageBox.Show("请先停止服务再卸载", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    return;
                }
                if (MessageBox.Show("确定要卸载服务吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //StopService("OpcDa2TdEngine");
                    await UninstallService("OpcDa2TdEngine");
                }
            }
            else
            {
                if (MessageBox.Show("确定要安装服务吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    await InstallService("OpcDa2TdEngine");
                }
            }
        }

        private async Task InstallService(string opcda2tdengine)
        {
            try
            {
                // 获取当前程序的绝对路径
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string binDirectory = Path.Combine(baseDirectory, "bin");
                string shawlPath = Path.Combine(binDirectory, "shawl.exe");
                string opcdaclientPath = Path.Combine(binDirectory, "opcdaclient.exe");

                // 检查 bin 目录以及 shawl.exe 和 opcdaclient.exe 是否存在
                if (!Directory.Exists(binDirectory) || !File.Exists(shawlPath) || !File.Exists(opcdaclientPath))
                {
                    MessageBox.Show("安装失败：缺少必要的文件或目录。请确保 bin 目录、shawl.exe 和 opcdaclient.exe 存在。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 构建安装命令，确保路径用双引号包裹
                string installCommand = $"\"{shawlPath}\" add --name \"{opcda2tdengine}\" --no-restart --cwd \"{binDirectory}\" -- \"{opcdaclientPath}\"";
                AppendToLogs($"服务 {opcda2tdengine} 安装命令。输出:\n {installCommand}");

                // 执行安装命令
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"/c \"{installCommand}\""; // 确保整个命令用双引号包裹
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {
                        AppendToLogs($"服务 {opcda2tdengine} 安装成功。输出: {output}");
                        MessageBox.Show($"服务 {opcda2tdengine} 安装成功。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        AppendToLogs($"服务 {opcda2tdengine} 安装失败。错误: {error}");
                        MessageBox.Show($"服务 {opcda2tdengine} 安装失败。错误: {error}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendToLogs($"服务安装过程中发生异常: {ex.Message}");
                MessageBox.Show($"服务安装过程中发生异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UninstallService(string opcda2tdengine)
        {
            try
            {
                // 构建卸载命令
                string uninstallCommand = $"sc delete {opcda2tdengine}";

                // 执行卸载命令
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = $"/c {uninstallCommand}";
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (process.ExitCode == 0)
                    {
                        AppendToLogs($"服务 {opcda2tdengine} 卸载成功。输出: {output}");
                        MessageBox.Show($"服务 {opcda2tdengine} 卸载成功。", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        AppendToLogs($"服务 {opcda2tdengine} 卸载失败。错误: {error}");
                        MessageBox.Show($"服务 {opcda2tdengine} 卸载失败。错误: {error}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendToLogs($"服务卸载过程中发生异常: {ex.Message}");
                MessageBox.Show($"服务卸载过程中发生异常: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string GenerateShortGuid(int length = 8)
        {
            return Guid.NewGuid().ToString("N").Substring(0, length); // 取前8位
        }

        private void LogClear_Click(object sender, EventArgs e)
        {
            logsText.Text = "";
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