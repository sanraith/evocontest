using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using evorace.WebApp.Common;
using evorace.Runner.Host.Extensions;
using ImpromptuInterface;
using System.Reflection;

namespace evorace.Runner.Host.Connection
{
    public sealed class WebAppConnector : IAsyncDisposable
    {
        public WebAppConnector(Uri loginUrl, Uri signalrUrl)
        {
            myCookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = myCookieContainer };
            myLoginUrl = loginUrl;
            mySignalrUrl = signalrUrl;
            myHttpClient = new HttpClient(handler);
        }

        public async Task Login(string email, string password)
        {
            string requestVerificationToken = await GetRequestVerificationToken(myLoginUrl).LogProgress("Getting request verification token");
            await PostLogin(email, password, requestVerificationToken).LogProgress("Logging in");
        }

        public async Task<IWorkerHubClient> ConnectToSignalR(IWorkerHubClient client)
        {
            var hubProxy = LoggerExtensions.LogProgress("Configuring signalR", () =>
            {
                myHubConn = new HubConnectionBuilder()
                    .WithUrl(mySignalrUrl, options => options.Cookies.Add(myCookieContainer.GetCookies(myLoginUrl)))
                    .Build();
                return MapClient(myHubConn, client);
            });

            await myHubConn!.StartAsync().LogProgress("Connecting to signalR");

            return hubProxy;
        }

        private async Task<string> GetRequestVerificationToken(Uri loginUri)
        {
            using var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = loginUri };
            using var response = await myHttpClient.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidProgramException();
            }

            var regex = new Regex(@"__RequestVerificationToken[^\>]*value=""(?'token'[^""]*)""");
            var contentString = await response.Content.ReadAsStringAsync();
            var requestVerificationToken = regex.Match(contentString).Groups["token"].Value;

            return requestVerificationToken;
        }

        private async Task PostLogin(string email, string password, string requestVerificationToken)
        {
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = myLoginUrl,
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "Input.Email", email },
                    { "Input.Password", password },
                    { "Input.RememberMe", "false" },
                    { "__RequestVerificationToken", requestVerificationToken }
                })
            };
            using var response = await myHttpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidProgramException();
            }
        }

        private static TClient MapClient<TClient>(HubConnection conn, TClient client) where TClient : class
        {
            var methods = typeof(TClient).GetMethods();
            foreach (var method in methods)
            {
                Func<object[], Task> methodInvoker;
                if (typeof(Task).IsAssignableFrom(method.ReturnType))
                {
                    methodInvoker = @params => (Task)method.Invoke(client, @params);
                }
                else
                {
                    methodInvoker = @params => Task.Run(() => method.Invoke(client, @params));
                }

                conn.On(method.Name, method.GetParameters().Select(x => x.ParameterType).ToArray(), methodInvoker);
            }

            return HubProxy<TClient>.Create(conn);
        }

        public async ValueTask DisposeAsync()
        {
            myHttpClient?.Dispose();
            if (myHubConn != null)
            {
                await myHubConn.DisposeAsync();
            }
        }

        private class HubProxy<TClient> : DynamicObject where TClient : class
        {
            private HubProxy(HubConnection hubConnection)
            {
                myHubConnection = hubConnection;
                myTargetMethods = typeof(TClient).GetMethods();
            }

            public static TClient Create(HubConnection hubConnection)
            {
                return new HubProxy<TClient>(hubConnection).ActLike<TClient>();
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

        private HubConnection? myHubConn;
        private readonly Uri myLoginUrl;
        private readonly Uri mySignalrUrl;
        private readonly HttpClient myHttpClient;
        private readonly CookieContainer myCookieContainer;
    }
}
