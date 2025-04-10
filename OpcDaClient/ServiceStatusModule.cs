using System;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json.Linq;
using OpcDaClient; // 新增：引用 TdEngine_Pub 和 OPCDA_Sub 所在的命名空间

// 新增：用于解析POST请求的模型
public class ServiceRequest {
    public string id { get; set; }
}

namespace OpcDaSubscription
{
    public class ServiceStatusModule : NancyModule
    {
        public ServiceStatusModule() : base("/apiv1")
        {
            Get("/service_Status", _ =>
            {
                // 返回服务状态的JSON数据
                var response = new
                {
                    result = true,
                    data = new
                    {
                        opcDaSub = Program.opcDaSubStatus,
                        tdEnginePub = Program.tdEnginePubStatus
                    },
                    message = "ok"
                };
                return Response.AsJson(response);
            });

            // 修改：POST /apiv1/start 接口，增加判断避免重复启动
            Post("/start", _ =>
            {
                var data = this.Bind<ServiceRequest>(); // 由 JObject 改为 ServiceRequest
                Console.WriteLine("start {0}", data.id);
                if (!string.IsNullOrEmpty(data.id))
                {
                    Console.WriteLine("DEBUG: 启动服务 API 接口，id 值为: " + data.id);
                    if (data.id == "tdEnginePub")
                    {
                        if (Program.tdEnginePubStatus)
                        {
                            var response = new
                            {
                                result = true,
                                data = new { alreadyStarted = true },
                                message = "tdEnginePub 已经启动"
                            };
                            return Response.AsJson(response);
                        }
                        Program.tdEnginePubInstance = new TdEngine_Pub(Program.opcDaSubInstance);
                        Program.tdEnginePubInstance.Start();
                        Program.tdEnginePubStatus = true;
                        var responseStarted = new
                        {
                            result = true,
                            data = new { start = true },
                            message = "ok"
                        };
                        return Response.AsJson(responseStarted);
                    }
                    else if (data.id == "opcDaSub")
                    {
                        if (Program.opcDaSubStatus)
                        {
                            var response = new
                            {
                                result = true,
                                data = new { alreadyStarted = true },
                                message = "opcDaSub 已经启动"
                            };
                            return Response.AsJson(response);
                        }
                        Program.opcDaSubInstance = new OPCDA_Sub();
                        Program.opcDaSubInstance.Start();
                        Program.opcDaSubStatus = true;
                        var responseStarted = new
                        {
                            result = true,
                            data = new { start = true },
                            message = "ok"
                        };
                        return Response.AsJson(responseStarted);
                    }
                }
                var invalidResponse = new
                {
                    result = false,
                    data = new { },
                    message = "invalid id"
                };
                return Response.AsJson(invalidResponse);
            });

            // 修改：POST /apiv1/stop 接口，增加判断避免重复停止
            Post("/stop", _ =>
            {
                var data = this.Bind<ServiceRequest>(); // 由 JObject 改为 ServiceRequest
                Console.WriteLine("stop {0}", data.id);
                if (!string.IsNullOrEmpty(data.id))
                {
                    Console.WriteLine("DEBUG: 停止服务 API 接口，id 值为: " + data.id);
                    if (data.id == "tdEnginePub")
                    {
                        if (!Program.tdEnginePubStatus)
                        {
                            var response = new
                            {
                                result = true,
                                data = new { alreadyStopped = true },
                                message = "tdEnginePub 已经停止"
                            };
                            return Response.AsJson(response);
                        }
                        Program.tdEnginePubInstance.Stop();
                        Program.tdEnginePubStatus = false;
                        var responseStopped = new
                        {
                            result = true,
                            data = new { stop = true },
                            message = "ok"
                        };
                        return Response.AsJson(responseStopped);
                    }
                    else if (data.id == "opcDaSub")
                    {
                        if (!Program.opcDaSubStatus)
                        {
                            var response = new
                            {
                                result = true,
                                data = new { alreadyStopped = true },
                                message = "opcDaSub 已经停止"
                            };
                            return Response.AsJson(response);
                        }
                        Program.opcDaSubInstance.Stop();
                        Program.opcDaSubStatus = false;
                        var responseStopped = new
                        {
                            result = true,
                            data = new { stop = true },
                            message = "ok"
                        };
                        return Response.AsJson(responseStopped);
                    }
                }
                var invalidResponse = new
                {
                    result = false,
                    data = new { },
                    message = "invalid id"
                };
                return Response.AsJson(invalidResponse);
            });
        }
    }
}

