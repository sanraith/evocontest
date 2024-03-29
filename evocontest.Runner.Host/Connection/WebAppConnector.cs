﻿using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using evocontest.WebApp.Common;
using evocontest.Runner.Host.Extensions;
using System.IO;
using evocontest.Runner.Host.Configuration;
using evocontest.WebApp.Common.Data;
using evocontest.WebApp.Common.Hub;
using evocontest.Runner.Host.Common.Utility;
using System.Text;

namespace evocontest.Runner.Host.Connection
{
    public sealed class WebAppConnector : IAsyncDisposable
    {
        public IWorkerHubServer? WorkerHubServer { get; private set; }

        public event EventHandler<Exception>? SignalRConnectionLost;

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
            myUploadMatchResultsUri = new Uri(hostUri, Constants.UploadMatchResultsRoute);
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                string requestVerificationToken = await GetRequestVerificationTokenAsync(myLoginUri).WithProgressLog("Getting request verification token");
                await PostLoginAsync(email, password, requestVerificationToken).WithProgressLog("Logging in");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error on login: " + ex);
                return false;
            }
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
                myHubConn.Closed += HubConnection_OnClosed;
                return HubProxy.Create(myHubConn, client);
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
            await using var jsonStream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<GetValidSubmissionsResult>(jsonStream);

            return result;
        }

        public async Task UploadMatchResults(MatchContainer matchResult)
        {
            try
            {
                using var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = myUploadMatchResultsUri,
                    Content = new StringContent(JsonSerializer.Serialize(matchResult), Encoding.UTF8, "application/json")
                };
                using var response = await myHttpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new InvalidOperationException();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during upload!!! {ex}");
            }
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

        private Task HubConnection_OnClosed(Exception exception)
        {
            if (myHubConn != null)
            {
                myHubConn.Closed -= HubConnection_OnClosed;
                if (exception != null)
                {
                    SignalRConnectionLost?.Invoke(this, exception);
                }
            }

            return Task.FromResult(true);
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
        private readonly Uri myUploadMatchResultsUri;
        private readonly Uri myDownloadSubmissionUri;
        private readonly Uri myGetValidSubmissionsUri;
        private readonly HttpClient myHttpClient;
        private readonly CookieContainer myCookieContainer;
    }
}
