using MQTTnet.Server;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Threading.Tasks;

namespace OpcDaSubscription
{
    public class MqttAuthentication : IMqttServerAuthenticationHandler
    {
        public Task<MqttServerAuthenticationResult> AuthenticateAsync(MqttServerAuthenticateEventArgs context)
        {
            if (context.UserName == "admin" && context.Password == "123456")
            {
                return Task.FromResult(new MqttServerAuthenticationResult { IsAuthenticated = true });
            }

            return Task.FromResult(new MqttServerAuthenticationResult { IsAuthenticated = false, ErrorMessage = "Invalid credentials." });
        }
    }
}
