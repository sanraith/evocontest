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

namespace evorace.Runner.Host.Connection
{
    public sealed class WebAppConnector : IAsyncDisposable
    {
        public WebAppConnector(Uri hostUri)
        {
            myCookieContainer = new CookieContainer();
            var handler = new HttpClientHandler { CookieContainer = myCookieContainer };
            myLoginUri = new Uri(hostUri, Constants.LoginRoute);
            mySignalrUri = new Uri(hostUri, Constants.WorkerHubRoute);
            myDownloadSubmissionUri = new Uri(hostUri, Constants.DownloadSubmissionRoute);
            myHttpClient = new HttpClient(handler);
        }

        public async Task Login(string email, string password)
        {
            string requestVerificationToken = await GetRequestVerificationToken(myLoginUri).LogProgress("Getting request verification token");
            await PostLogin(email, password, requestVerificationToken).LogProgress("Logging in");
        }

        public async Task<IWorkerHubServer> ConnectToSignalR(IWorkerHubClient client)
        {
            var hubProxy = Extensions.LoggerExtensions.LogProgress("Configuring signalR", () =>
            {
                myHubConn = new HubConnectionBuilder()
                    .WithUrl(mySignalrUri, options => options.Cookies.Add(myCookieContainer.GetCookies(myLoginUri)))
                    .WithAutomaticReconnect()
                    .ConfigureLogging(options => options.AddConsole().SetMinimumLevel(LogLevel.Error))
                    .Build();
                return HubProxy.Create<IWorkerHubServer, IWorkerHubClient>(myHubConn, client);
            });

            await myHubConn!.StartAsync().LogProgress("Connecting to signalR");

            return hubProxy;
        }

        public async Task<FileInfo> DownloadSubmission(string submissionId)
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
            using var response = await myHttpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException();
            }

            var fileName = response.Content.Headers.ContentDisposition.FileName;
            var targetDirectory = new DirectoryInfo($"temp\\{submissionId}");
            var fileInfo = new FileInfo(Path.Combine(targetDirectory.FullName, fileName));
            if (!targetDirectory.Exists) { targetDirectory.Create(); }

            using var readStream = await response.Content.ReadAsStreamAsync();
            using var writeStream = File.Create(fileInfo.FullName);
            await readStream.CopyToAsync(writeStream);

            return fileInfo;
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
