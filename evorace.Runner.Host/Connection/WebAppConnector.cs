using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using evorace.WebApp.Common;
using evorace.Runner.Host.Extensions;
using System.IO;
using evorace.Runner.Host.Configuration;
using evorace.Runner.Common.Utility;

namespace evorace.Runner.Host.Connection
{
    public sealed class WebAppConnector : IAsyncDisposable
    {
        public IWorkerHubServer? WorkerHubServer { get; private set; }

        public WebAppConnector(HostConfiguration config)
        {
            myCookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = myCookieContainer };
            myHttpClient = new HttpClient(handler);

            Uri hostUri = new Uri(config.HostUrl);
            myLoginUri = new Uri(hostUri, Constants.LoginRoute);
            mySignalrUri = new Uri(hostUri, Constants.WorkerHubRoute);
            myDownloadSubmissionUri = new Uri(hostUri, Constants.DownloadSubmissionRoute);
        }

        public async Task Login(string email, string password)
        {
            string requestVerificationToken = await GetRequestVerificationToken(myLoginUri).WithProgressLog("Getting request verification token");
            await PostLogin(email, password, requestVerificationToken).WithProgressLog("Logging in");
        }

        public IWorkerHubServer InitSignalR(IWorkerHubClient client)
        {
            var hubProxy = Extensions.LoggerExtensions.WithProgressLog("Configuring signalR", () =>
            {
                myHubConn = new HubConnectionBuilder()
                    .WithUrl(mySignalrUri, options => options.Cookies.Add(myCookieContainer.GetCookies(myLoginUri)))
                    .WithAutomaticReconnect()
                    .ConfigureLogging(options => options.AddConsole().SetMinimumLevel(LogLevel.Error))
                    .Build();
                return HubProxy.Create<IWorkerHubServer, IWorkerHubClient>(myHubConn, client);
            });
            WorkerHubServer = hubProxy;

            return hubProxy;
        }

        public Task StartSignalR()
        {
            return myHubConn!.StartAsync().WithProgressLog("Connecting to signalR");
        }

        public Task StopSignalR()
        {
            return myHubConn!.StopAsync().WithProgressLog("Stopping signalR");
        }

        public async Task<DisposableValue<(string FileName, Stream DownloadStream)>> DownloadSubmission(string submissionId)
        {
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = myDownloadSubmissionUri,
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "submissionId", submissionId}
                })
            };

            var response = await myHttpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                response.Dispose();
                throw new InvalidOperationException();
            }

            var fileName = response.Content.Headers.ContentDisposition.FileName;
            var downloadStream = await response.Content.ReadAsStreamAsync();

            return DisposableValue.Create((fileName, downloadStream), downloadStream, response);
        }

        private async Task<string> GetRequestVerificationToken(Uri loginUri)
        {
            using var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = loginUri };
            using var response = await myHttpClient.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException();
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
                RequestUri = myLoginUri,
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
                throw new InvalidOperationException();
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
        private readonly Uri myLoginUri;
        private readonly Uri mySignalrUri;
        private readonly Uri myDownloadSubmissionUri;
        private readonly HttpClient myHttpClient;
        private readonly CookieContainer myCookieContainer;
    }
}
