using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LoginTest
{
    public sealed class WebAppConnector : IDisposable
    {
        public WebAppConnector()
        {
            CookieContainer cookieContainer = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler { CookieContainer = cookieContainer };
            myHttpClient = new HttpClient(handler);
        }

        public async Task Login(string email, string password)
        {
            var loginUri = new Uri("https://localhost:44359/Identity/Account/Login");

            string requestVerificationToken = await GetRequestVerificationToken(loginUri);
            await PostLogin(email, password, loginUri, requestVerificationToken);
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

        private async Task PostLogin(string email, string password, Uri loginUri, string requestVerificationToken)
        {
            using var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = loginUri,
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
        }

        private readonly HttpClient myHttpClient;
    }
}
