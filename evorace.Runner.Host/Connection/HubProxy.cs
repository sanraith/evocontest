using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Dynamic;
using System.Linq;
using ImpromptuInterface;
using System.Reflection;
using System.Threading.Tasks;

namespace evorace.Runner.Host.Connection
{
    public sealed class HubProxy : DynamicObject
    {
        private HubProxy(HubConnection hubConnection, Type proxyType)
        {
            myHubConnection = hubConnection;
            myTargetMethods = proxyType.GetMethods();
        }

        public static TServer Create<TServer>(HubConnection hubConnection) 
            where TServer : class
        {
            return Create<TServer, object>(hubConnection, null);
        }

        public static TServer Create<TServer, TClient>(HubConnection hubConnection, TClient? client) 
            where TServer : class
            where TClient : class
        {
            var proxy = new HubProxy(hubConnection, typeof(TServer));
            if (client != null)
            {
                proxy.BindClient(client);
            }

            return proxy.ActLike<TServer>();
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object? result)
        {
            result = null;
            var targetMethodName = binder.Name;

            if (!TryGetTargetMethod(targetMethodName, args, out _))
            {
                return false;
            }

            result = myHubConnection.SendCoreAsync(targetMethodName, args);
            return true;
        }

        private void BindClient<TClient>(TClient client)
        {
            var methods = typeof(TClient).GetMethods();
            foreach (var method in methods)
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

        private bool TryGetTargetMethod(string targetMethodName, object[] args, out MethodInfo targetMethodInfo)
        {
            targetMethodInfo = myTargetMethods
                .Where(x => x.Name == targetMethodName)
                .FirstOrDefault(m =>
                {
                    var targetParameterInfos = m.GetParameters();
                    if (targetParameterInfos.Length != args.Length)
                    {
                        return false;
                    }

                    var areParameterTypesMatch = targetParameterInfos
                        .Zip(args, (a, b) => (Source: b?.GetType(), Target: a.ParameterType))
                        .Select(pair =>
                        {
                            (var source, Type? target) = pair;
                            if (source == null)
                            {
                                var isTargetNullable = !target.IsValueType || Nullable.GetUnderlyingType(target) != null;
                                target = isTargetNullable ? null : target;
                            }
                            return (Source: source, Target: target);
                        })
                        .All(p => p.Source == p.Target);

                    return areParameterTypesMatch;
                });

            return targetMethodInfo != null;
        }

        private readonly MethodInfo[] myTargetMethods;
        private readonly HubConnection myHubConnection;
    }
}
