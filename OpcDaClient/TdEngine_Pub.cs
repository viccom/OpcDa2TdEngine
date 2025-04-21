using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Nett;
using Opc.Da;
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
        private const int MaxQueueSize = 100000;

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

                    // 创建超级表
                    var sTableList = new string[] {"bool", "int", "uint", "float", "double", "string"};
                    foreach (var sTable in sTableList)
                    {
                        try
                        {
                            var sTableName = $"opc_{sTable}";
                            // Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 正在创建超级表: {config.TdEngine.Dbname}.{sTableName}");
                            // Console.WriteLine($"CREATE STABLE IF NOT EXISTS {config.TdEngine.Dbname}.{sTableName} (ts TIMESTAMP, val {GetTdEngineType(sTable)}) TAGS (tagName BINARY(100))");
                            client.Exec($"CREATE STABLE IF NOT EXISTS {config.TdEngine.Dbname}.{sTableName} (ts TIMESTAMP, val {GetTdEngineType(sTable)}) TAGS (tagName BINARY(100))");
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 成功创建超级表: {config.TdEngine.Dbname}.{sTableName}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 警告: 创建超级表索引失败 - {ex.Message}");
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
                                
                                //重构新的数据字典
                                var newQueueMap = new Dictionary<string, List<ItemValueResult>>(); 
                                foreach (var kvp in dataMap)
                                {
                                    string key = kvp.Value.DiagnosticInfo;
                                    ItemValueResult value = kvp.Value;
                                    if (newQueueMap.TryGetValue(key, out List<ItemValueResult> existingList))
                                    {
                                        existingList.Add(value); // 添加到现有列表
                                    }
                                    else
                                    {
                                        newQueueMap.Add(key, new List<ItemValueResult> { value }); 
                                    }
                                }
                                
                                // 写数据库开始
                                // 对每张超级表生成单独的SQL语句进行插入
                                foreach (var kv in newQueueMap)
                                {
                                    string superTable = "opc_" + kv.Key;
                                    var groups = kv.Value.GroupBy(item => item.ItemPath);
    
                                    // 构造多子表插入语句
                                    List<string> insertSegments = new List<string>();
                                    foreach (var group in groups)
                                    {
                                        string childTable = group.Key;
                                        List<string> rows = new List<string>();
                                        foreach (var item in group)
                                        {
                                            long ts = (long)(item.Timestamp.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                                            object value = ConvertToTdEngineValue(item.Value);
                                            rows.Add($"(\"{ts}\", {value})");
                                        }
                                        string segment = $"{childTable} USING {superTable} TAGS (\"{childTable}\") VALUES {string.Join(", ", rows)}";
                                        insertSegments.Add(segment);
                                    }
    
                                    if (insertSegments.Count > 0)
                                    {
                                        // 正确的TDengine多子表插入语法
                                        int batchSize = 1000;
                                        for (int i = 0; i < insertSegments.Count; i += batchSize)
                                        {
                                            int endIndex = Math.Min(i + batchSize, insertSegments.Count);
                                            List<string> batchSegments = insertSegments.GetRange(i, endIndex - i);

                                            string sql = "INSERT INTO " + string.Join(", ", batchSegments) + ";";
                                            // Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 执行SQL: {sql}");

                                            try
                                            {
                                                long affectedRows = client.Exec(sql);
                                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] 成功写入 {affectedRows} 行数据到超级表 {superTable}");
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [ERROR] 写入失败: {ex.Message}");
                                            }
                                        }
                                    }
                                }

                                // 写数据库结束
                            }
                            // 缩短等待间隔并检查运行状态
                            for (int i = 0; i < 5; i++)
                            {
                                if (!_running) break;
                                Thread.Sleep(3);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss zzz}] 数据处理错误: {ex.Message}");
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
                case "uint": return "INT UNSIGNED";
                case "int64": return "BIGINT";
                case "uint64": return "BIGINT UNSIGNED";
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
