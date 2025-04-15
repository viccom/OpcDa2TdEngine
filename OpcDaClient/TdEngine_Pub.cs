using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nett;
using TDengine;
using TDengine.Driver;
using TDengine.Driver.Client;

// 命名空间OpcDaClient内部的TdEngine_Pub类
namespace OpcDaClient
{
    // TdEngine_Pub类负责与TDengine数据库进行通信
    internal class TdEngine_Pub
    {
        // 标记线程是否正在运行
        private bool _running = true;
        // TDengine数据库操作线程
        private Thread _tdThread;
        // OPCDA_Sub实例，用于数据订阅
        private readonly OPC_LiteDB _opcDaSub;
        // 配置信息
        private OPC_LiteDB.Config config;

        // 定义最大队列大小常量
        private const int MaxQueueSize = 1000;

        // 构造函数，接收一个OPCDA_Sub实例
        public TdEngine_Pub(OPC_LiteDB opcDaSub)
        {
            _opcDaSub = opcDaSub;
        }

        // 启动TDengine线程
        public void Start()
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 正在启动TDengine线程...");
            _tdThread = new Thread(StartTdEngineClient);
            _tdThread.Start();
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TDengine线程已启动，线程ID: {_tdThread.ManagedThreadId}");
        }

        // 停止TDengine线程
        public void Stop()
        {
            _running = false;
            if (_tdThread != null && _tdThread.IsAlive)
            {
                _tdThread.Join(2000);
            }
        }

        // TDengine客户端启动方法
        private void StartTdEngineClient()
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TDengine线程开始执行");
            TDengine.Driver.ITDengineClient client = null;

            // 新增：重试间隔配置
            int retryInterval = 5000;
            const int maxRetryInterval = 60000;

            while (_running)
            {
                try
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 正在读取TDengine配置文件...");
                    try
                    {
                        config = Toml.ReadFile<OPC_LiteDB.Config>("config.toml");

                        if (config?.TdEngine == null)
                        {
                            throw new Exception("配置文件中缺少TdEngine配置节");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 错误: 读取配置文件失败 - {ex.Message}");
                        throw;
                    }

                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] TDengine配置节: Host={config.TdEngine.Host}, Port={config.TdEngine.Port}, Dbname={config.TdEngine.Dbname}");

                    try
                    {
                        var connectionString = $"protocol=WebSocket;host={config.TdEngine.Host};port={config.TdEngine.Port};useSSL=false;username={config.TdEngine.Username};password={config.TdEngine.Password}";
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 正在连接TDengine: {connectionString.Replace("password=" + config.TdEngine.Password, "password=******")}");
                        var builder = new ConnectionStringBuilder(connectionString);
                        client = DbDriver.Open(builder);
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 成功连接到TDengine");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 错误: 连接TDengine失败 - {ex.Message}");
                        throw;
                    }

                    // 检查并创建数据库
                    try
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 正在创建数据库: {config.TdEngine.Dbname}");
                        client.Exec($"CREATE DATABASE IF NOT EXISTS {config.TdEngine.Dbname}");
                        client.Exec($"USE {config.TdEngine.Dbname}");
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 成功创建并使用数据库: {config.TdEngine.Dbname}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 错误: 数据库操作失败 - {ex.Message}");
                        throw;
                    }

                    // 读取csv文件创建表结构
                    var csvLines = File.ReadAllLines("items.csv");
                    foreach (var line in csvLines.Skip(1))
                    {
                        var parts = line.Split(',');
                        if (parts.Length >= 4)
                        {
                            var tableName = parts[0];
                            var fieldType = parts[2].ToLower();

                            using (var stmt = client.StmtInit())
                            {
                                var createTableSql = $"CREATE TABLE IF NOT EXISTS {config.TdEngine.Dbname}.{tableName} (ts TIMESTAMP, val {GetTdEngineType(fieldType)})";
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 正在创建表: {config.TdEngine.Dbname}.{tableName} 使用SQL: {createTableSql}");
                                //stmt.Prepare(createTableSql);
                                //stmt.Exec();
                                client.Exec(createTableSql);
                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 成功创建表: {config.TdEngine.Dbname}.{tableName}");
                            }
                        }
                    }

                    // 数据处理循环
                    while (_running)
                    {
                        try
                        {
                            if (_opcDaSub.TryGetData(out var dataMap))
                            {
                                if (dataMap.Count > MaxQueueSize)
                                {
                                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 警告: 数据队列大小({dataMap.Count})超过最大限制({MaxQueueSize})");
                                    continue;
                                }

                                // 增加运行状态检查
                                if (!_running) break;

                                foreach (var kvp in dataMap)
                                {
                                    var tableName = kvp.Key;
                                    var item = kvp.Value;

                                    try
                                    {
                                        using (var stmt = client.StmtInit())
                                        {
                                            stmt.Prepare($"INSERT INTO {tableName} VALUES (?, ?)");
                                            stmt.BindRow(new object[] { item.Timestamp, ConvertToTdEngineValue(item.Value) });
                                            stmt.AddBatch();
                                            stmt.Exec();
                                        }
                                        // Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 正确: 插入数据到表{tableName}成功");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 错误: 插入数据到表{tableName}失败 - {ex.Message}\n数据: Timestamp={item.Timestamp}, Value={item.Value}");
                                    }
                                }
                                
                            }
                            // 缩短等待间隔并检查运行状态
                            for (int i = 0; i < 10; i++)
                            {
                                if (!_running) break;
                                Thread.Sleep(10);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 数据处理错误: {ex.Message}");
                            break; // 新增：跳出内层循环触发重连
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 连接或配置错误: {ex.Message}");
                    Thread.Sleep(retryInterval); // 新增：指数退避
                    retryInterval = Math.Min(retryInterval * 2, maxRetryInterval); // 新增：指数退避
                }
                finally
                {
                    client?.Dispose();
                    client = null;
                }
            }
        }

        // 根据数据类型获取TDengine中的类型
        private string GetTdEngineType(string type)
        {
            switch (type)
            {
                case "bool": return "BOOL";
                case "int": return "INT";
                case "float": return "FLOAT";
                case "double": return "DOUBLE";
                case "string": return "BINARY(100)";
                default: return "DOUBLE";
            }
        }

        // 将数据转换为TDengine中的值
        private object ConvertToTdEngineValue(object value)
        {
            if (value == null) return DBNull.Value;

            try
            {
                return Convert.ChangeType(value, value.GetType());
            }
            catch
            {
                return value.ToString();
            }
        }
    }
}