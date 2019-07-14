using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using evorace.WebApp.Common;

namespace LoginTest
{
    public sealed class WebAppConnector : IDisposable
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
            string requestVerificationToken = await LogProgress("Getting request verification token", GetRequestVerificationToken(myLoginUrl));
            await LogProgress("Logging in", PostLogin(email, password, requestVerificationToken));
        }

        public async Task ConnectToSignalR(IWorkerHubClient client)
        {
            myHubConn = new HubConnectionBuilder()
                .WithUrl(mySignalrUrl,
                    options => options.Cookies.Add(myCookieContainer.GetCookies(myLoginUrl)))
                .Build();
            MapClient(myHubConn, client);

            await LogProgress("Connecting to signalR", myHubConn.StartAsync());
        }

        private async Task<string> GetRequestVerificationToken(Uri loginUri)
        {
            using var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = loginUri };
            using var response = await myHttpClient.SendAsync(request);

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
            using var _ = await myHttpClient.SendAsync(request);
        }

        public void Dispose()
        {
            myHttpClient?.Dispose();
            myHubConn?.DisposeAsync().GetAwaiter().GetResult();
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

        private static async Task<T> LogProgress<T>(string message, Task<T> task)
        {
            Console.Write($"{message}... ");
            var result = await task;
            Console.WriteLine("done.");
            return result;
        }

        private static Task LogProgress(string message, Task task)
        {
            return LogProgress(message, task.ContinueWith(_ => Task.FromResult(true)));
        }

        private HubConnection myHubConn;
        private readonly Uri myLoginUrl;
        private readonly Uri mySignalrUrl;
        private readonly HttpClient myHttpClient;
        private readonly CookieContainer myCookieContainer;
    }
}
