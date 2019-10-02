using evocontest.WebApp.Common;
using evocontest.WebApp.Common.Hub;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace evocontest.Runner.Host.Connection
{
    public class HubProxy : IWorkerHubServer
    {
        public Task SendMessage(string status)
        {
            return myHubConnection.SendCoreAsync(nameof(SendMessage), new[] { status });
        }

        public Task UpdateStatus(string submissionId, ValidationStateEnum state, string? error)
        {
            return myHubConnection.SendCoreAsync(nameof(UpdateStatus), new object[] { submissionId, state, error });
        }

        private HubProxy(HubConnection hubConnection)
        {
            myHubConnection = hubConnection;
        }

        public static HubProxy Create<TServer>(HubConnection hubConnection)
            where TServer : class
        {
            return Create<TServer>(hubConnection, null);
        }

        public static HubProxy Create<TClient>(HubConnection hubConnection, TClient? client)
            where TClient : class
        {
            var proxy = new HubProxy(hubConnection);
            if (client != null)
            {
                proxy.BindClient(client);
            }

            return proxy;
        }

        private void BindClient<TClient>(TClient client)
        {
            foreach (var method in typeof(TClient).GetMethods())
            {
                Func<object[], Task> methodInvoker;
                if (typeof(Task).IsAssignableFrom(method.ReturnType))
                {
                    methodInvoker = @params => (Task?)method.Invoke(client, @params) ?? Task.CompletedTask;
                }
                else
                {
                    methodInvoker = @params => Task.Run(() => method.Invoke(client, @params));
                }

                myHubConnection.On(method.Name, method.GetParameters().Select(x => x.ParameterType).ToArray(), methodInvoker);
            }
        }

        private readonly HubConnection myHubConnection;
    }
}
