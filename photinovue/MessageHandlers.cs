using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Photino.HelloPhotino.Vue.Handlers
{
    public static class MessageHandlers
    {
        public static string InstallStatus(dynamic message)
        {
            Console.WriteLine("收到前端消息: {0}", message);
            var type = message.type;
            var reqid = message.reqid;

            var serviceNames = new[] { "opcda2tdengine", "mochi_broker" };
            // 查询服务状态
            var serviceStatus = new Dictionary<string, bool>();
            foreach (var serviceName in serviceNames)
            {
                try
                {
                    var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == serviceName);
                    serviceStatus[serviceName] = service != null;
                }
                catch
                {
                    serviceStatus[serviceName] = false;
                }
            }

            // 构造返回结果
            var result = serviceStatus.All(kvp => kvp.Value);
            var response = new
            {
                result = result,
                type = type,
                payload = serviceStatus,
                reqid = reqid
            };

            return JsonConvert.SerializeObject(response);
        }
        
        public static string ServiceStatus(dynamic message)
        {
            Console.WriteLine("2.1收到前端消息: {0}", message);
            var type = message.type;
            var reqid = message.reqid;

            var serviceNames = new[] { "opcda2tdengine", "mochi_broker" };

            // 查询服务状态
            var serviceStatus = new Dictionary<string, bool>();
            foreach (var serviceName in serviceNames)
            {
                try
                {
                    var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == serviceName);
                    serviceStatus[serviceName] = service != null && service.Status == ServiceControllerStatus.Running;
                }
                catch
                {
                    serviceStatus[serviceName] = false;
                }
            }

            // 构造返回结果
            var result = serviceStatus.All(kvp => kvp.Value);
            var response = new
            {
                result = result,
                type = type,
                payload = serviceStatus,
                reqid = reqid
            };

            return JsonConvert.SerializeObject(response);
        }

        // 处理 Start 类型的消息
        public static string HandleStart(dynamic message)
        {
            var serviceNames = new[] { "opcda2tdengine", "mochi_broker" };
            var results = new List<string>();
            var resultb = new List<bool>();
            var reqid = message.reqid;
            var type = message.type;

            foreach (var serviceName in serviceNames)
            {
                try
                {
                    var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == serviceName);
                    if (service != null && service.Status != ServiceControllerStatus.Running)
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                        results.Add($"Service \"{serviceName}\" started successfully.");
                        resultb.Add(true);
                    }
                    else
                    {
                        results.Add($"Service \"{serviceName}\" is already running. Skipping start operation.");
                        resultb.Add(false);
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"Failed to start service \"{serviceName}\": {ex.Message}");
                }
            }
            bool result = resultb.All(r => r);
            var response = new
            {
                result = result,
                type = type,
                payload = string.Join("\n", results),
                reqid = reqid
            };

            return JsonConvert.SerializeObject(response);
        }

        // 处理 Stop 类型的消息
        public static string HandleStop(dynamic message)
        {
            var serviceNames = new[] { "opcda2tdengine", "mochi_broker" };
            var results = new List<string>();
            var resultb = new List<bool>();
            var reqid = message.reqid;
            var type = message.type;

            foreach (var serviceName in serviceNames)
            {
                try
                {
                    var service = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == serviceName);
                    if (service != null && service.Status != ServiceControllerStatus.Stopped)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                        results.Add($"Service \"{serviceName}\" stopped successfully.");
                        resultb.Add(true);
                    }
                    else
                    {
                        results.Add($"Service \"{serviceName}\" is already stopped. Skipping stop operation.");
                        resultb.Add(false);
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"Failed to stop service \"{serviceName}\": {ex.Message}");
                }
            }

            bool result = resultb.All(r => r);
            var response = new
            {
                result = result,
                message = type,
                payload = string.Join("\n", results),
                reqid = reqid
            };

            return JsonConvert.SerializeObject(response);
        }

        // 处理 install 类型的消息
        public static async Task<string> HandleInstall(dynamic message)
        {
            var reqid = message.reqid;
            var serviceName = "opcda2tdengine"; // 假设服务名称为 opcda2tdengine

            try
            {
                await InstallService(serviceName);
                var response = new
                {
                    result = true,
                    message = "服务安装成功",
                    payload = $"服务 \"{serviceName}\" 已成功安装。",
                    reqid = reqid
                };
                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    result = false,
                    message = "服务安装失败",
                    payload = $"安装服务 \"{serviceName}\" 时发生错误: {ex.Message}",
                    reqid = reqid
                };
                return JsonConvert.SerializeObject(response);
            }
        }

        // 处理 uninstall 类型的消息
        public static async Task<string> HandleUninstall(dynamic message)
        {
            var reqid = message.reqid;
            var serviceName = "opcda2tdengine"; // 假设服务名称为 opcda2tdengine

            try
            {
                await UninstallService(serviceName);
                var response = new
                {
                    result = true,
                    message = "服务卸载成功",
                    payload = $"服务 \"{serviceName}\" 已成功卸载。",
                    reqid = reqid
                };
                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    result = false,
                    message = "服务卸载失败",
                    payload = $"卸载服务 \"{serviceName}\" 时发生错误: {ex.Message}",
                    reqid = reqid
                };
                return JsonConvert.SerializeObject(response);
            }
        }

        // 新增：安装服务的方法
        private static async Task InstallService(string serviceName)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string binDirectory = Path.Combine(baseDirectory, "bin");
            string shawlPath = Path.Combine(binDirectory, "shawl.exe");
            string opcdaclientPath = Path.Combine(binDirectory, "opcdaclient.exe");

            if (!Directory.Exists(binDirectory) || !File.Exists(shawlPath) || !File.Exists(opcdaclientPath))
            {
                throw new Exception("安装失败：缺少必要的文件或目录。请确保 bin 目录、shawl.exe 和 opcdaclient.exe 存在。");
            }

            string installCommand = $"\"{shawlPath}\" add --name \"{serviceName}\" --no-restart --cwd \"{binDirectory}\" -- \"{opcdaclientPath}\"";

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c \"{installCommand}\"";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"安装服务失败: {error}");
                }
            }
        }

        // 新增：卸载服务的方法
        private static async Task UninstallService(string serviceName)
        {
            string uninstallCommand = $"sc delete {serviceName}";

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

                if (process.ExitCode != 0)
                {
                    throw new Exception($"卸载服务失败: {error}");
                }
            }
        }

    }
}
