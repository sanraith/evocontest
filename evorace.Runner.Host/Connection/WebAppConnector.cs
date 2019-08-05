﻿using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
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
            myGetValidSubmissionsUri = new Uri(hostUri, Constants.GetValidSubmissionsRoute);
        }

        public async Task LoginAsync(string email, string password)
        {
            string requestVerificationToken = await GetRequestVerificationTokenAsync(myLoginUri).WithProgressLog("Getting request verification token");
            await PostLoginAsync(email, password, requestVerificationToken).WithProgressLog("Logging in");
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

        public Task StartSignalRAsync()
        {
            if (myHubConn == null) { throw new InvalidOperationException("SignalR is not initialized!"); }

            return myHubConn.StartAsync().WithProgressLog("Connecting to signalR");
        }

        public Task StopSignalRAsync()
        {
            if (myHubConn == null) { throw new InvalidOperationException("SignalR is not initialized!"); }

            return myHubConn.StopAsync().WithProgressLog("Stopping signalR");
        }

        public async Task<DisposableValue<(string FileName, Stream DownloadStream)>> DownloadSubmissionAsync(string submissionId)
        {
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = myDownloadSubmissionUri,
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "submissionId", submissionId }
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

        public async Task<GetValidSubmissionsResult> GetValidSubmissionsAsync()
        {
            using var request = new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = myGetValidSubmissionsUri };
            using var response = await myHttpClient.SendAsync(request);
            var jsonResponse = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<GetValidSubmissionsResult>(jsonResponse);

            return result;
        }

        private async Task<string> GetRequestVerificationTokenAsync(Uri loginUri)
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

        private async Task PostLoginAsync(string email, string password, string requestVerificationToken)
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
        private readonly Uri myGetValidSubmissionsUri;
        private readonly HttpClient myHttpClient;
        private readonly CookieContainer myCookieContainer;
    }
}
