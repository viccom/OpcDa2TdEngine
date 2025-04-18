using System;

namespace Photino.HelloPhotino.Vue.Handlers
{
    public static class MessageHandlers
    {
        // 处理 TypeA 类型的消息
        public static string HandleTypeA(dynamic message)
        {
            return $"TypeA : \"{message.type}\"";
        }

        // 处理 TypeB 类型的消息
        public static string HandleTypeB(dynamic message)
        {
            return $"TypeB : \"{message.type}\"";
        }
    }
}