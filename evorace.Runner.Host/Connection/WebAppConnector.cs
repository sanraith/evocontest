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

namespace evorace.Runner.Host.Connection
{
    public sealed class WebAppConnector : IAsyncDisposable
    {
        public WebAppConnector(Uri loginUrl, Uri signalrUrl)
        {
            myCookieContainer = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler { CookieContainer = myCookieContainer };
            myLoginUrl = loginUrl;
            mySignalrUrl = signalrUrl;
            myHttpClient = new HttpClient(handler);
        }

        public async Task Login(string email, string password)
        {
            string requestVerificationToken = await GetRequestVerificationToken(myLoginUrl).LogProgress("Getting request verification token");
            await PostLogin(email, password, requestVerificationToken).LogProgress("Logging in");
        }

        public async Task ConnectToSignalR(IWorkerHubClient client)
        {
            LoggerExtensions.LogProgress("Configuring signalR", () =>
            {
                myHubConn = new HubConnectionBuilder()
                    .WithUrl(mySignalrUrl, options => options.Cookies.Add(myCookieContainer.GetCookies(myLoginUrl)))
                    .Build();
                MapClient(myHubConn, client);
            });

            await myHubConn!.StartAsync().LogProgress("Connecting to signalR");
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

        private static void MapClient<TClient>(HubConnection conn, TClient client) where TClient : class
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
            public HubProxy(HubConnection hubConnection)
            {
                myHubConnection = hubConnection;
                myClientType = typeof(TClient);
            }

            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object? result)
            {
                result = null;
                var targetMethodName = binder.Name;
                var methods = myClientType.GetMethods();


                var targetMethod = methods
                    .Where(x => x.Name == targetMethodName)
                    .FirstOrDefault(m =>
                    {
                        var targetParameterInfos = m.GetParameters();
                        if (targetParameterInfos.Length != args.Length)
                        {
                            return false;
                        }

                        var parameterPairs = targetParameterInfos
                            .Select(x => x.ParameterType)
                            .Zip(args.Select(x => x?.GetType()))
                            .Select(p => p.Second == null ?
                            (!p.First.IsValueType || Nullable.GetUnderlyingType(p.First) != null ? null : p.First, null) : p);
                        var areParameterTypesMatch = parameterPairs.All(p => p.First == p.Second);

                        return areParameterTypesMatch;
                    });

                if (targetMethod == null)
                {
                    return false;
                }

                result = myHubConnection.SendCoreAsync(targetMethodName, args);
                return true;
            }

            private readonly Type myClientType;
            private readonly HubConnection myHubConnection;
        }

        private HubConnection? myHubConn;
        private readonly Uri myLoginUrl;
        private readonly Uri mySignalrUrl;
        private readonly HttpClient myHttpClient;
        private readonly CookieContainer myCookieContainer;
    }
}
