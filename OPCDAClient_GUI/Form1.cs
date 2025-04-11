using System;
using System.Data;
using System.IO;
using System.ServiceProcess;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Threading;
using System.Timers;

namespace OPCDAClient_GUI
{
    public partial class Form1 : Form
    {
        // 新增：在Form1中添加对服务状态检查的支持
        private Thread _checkStatusThread;
        private System.Timers.Timer _statusTimer;
        private HttpClient _httpClient;
        public Form1()
        {
            InitializeComponent();
            InitializeStatusChecker();
            // 启动服务状态检查线程
            InitializeInstallChecker();
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
                Thread.Sleep(5000); // 每5秒检查一次
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