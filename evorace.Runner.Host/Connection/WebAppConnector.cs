using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
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

        public async Task<IWorkerHubServer> ConnectToSignalR(IWorkerHubClient client)
        {
            var hubProxy = LoggerExtensions.LogProgress("Configuring signalR", () =>
            {
                myHubConn = new HubConnectionBuilder()
                    .WithUrl(mySignalrUrl, options => options.Cookies.Add(myCookieContainer.GetCookies(myLoginUrl)))
                    .WithAutomaticReconnect()
                    .Build();
                return HubProxy.Create<IWorkerHubServer, IWorkerHubClient>(myHubConn, client);
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

        public async ValueTask DisposeAsync()
        {
            myHttpClient?.Dispose();
            if (myHubConn != null)
            {
                await myHubConn.DisposeAsync();
            }
        }

        private HubConnection? myHubConn;
        private readonly Uri myLoginUrl;
        private readonly Uri mySignalrUrl;
        private readonly HttpClient myHttpClient;
        private readonly CookieContainer myCookieContainer;
    }
}
