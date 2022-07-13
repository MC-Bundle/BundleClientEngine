using Bundle.Client.Json;
using Bundle.Client.Options;
using Bundle.Runtime;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;

namespace Bundle.Client.Authorization
{
    public class Microsoft
    {
        private readonly MicrosoftOptions _microsoftOptions;
        private readonly ITcpClientFactory _tcpClientFactory;

        public Microsoft(ITcpClientFactory tcpClientFactory, IOptions<MicrosoftOptions> options)
        {
            _tcpClientFactory = tcpClientFactory;
            _microsoftOptions = options.Value;
        }
        public string SignInUrl { get { return _microsoftOptions.SigninUrl; } }

        public string GetSignInUrlWithHint(string loginHint)
        {
            return _microsoftOptions.SigninUrl + "&login_hint=" + Uri.EscapeDataString(loginHint);
        }

        public LoginResponse RequestAccessToken(string code)
        {
            string postData = "client_id={0}&grant_type=authorization_code&redirect_uri=https%3A%2F%2Fmccteam.github.io%2Fredirect.html&code={1}";
            postData = string.Format(postData, _microsoftOptions.ClientId, code);
            return RequestToken(postData);
        }

        public LoginResponse RefreshAccessToken(string refreshToken)
        {
            string postData = "client_id={0}&grant_type=refresh_token&redirect_uri=https%3A%2F%2Fmccteam.github.io%2Fredirect.html&refresh_token={1}";
            postData = string.Format(postData, _microsoftOptions.ClientId, refreshToken);
            return RequestToken(postData);
        }

        private LoginResponse RequestToken(string postData)
        {
            var request = new ProxiedWebRequest(_microsoftOptions.TokenUrl, _tcpClientFactory);
            request.UserAgent = "MCC/0";
            var response = request.Post("application/x-www-form-urlencoded", postData);
            var jsonData = JsonConv.ParseJson(response.Body);

            // Error handling
            if (jsonData.Properties.ContainsKey("error"))
            {
                throw new Exception(jsonData.Properties["error_description"].StringValue);
            }
            else
            {
                string accessToken = jsonData.Properties["access_token"].StringValue;
                string refreshToken = jsonData.Properties["refresh_token"].StringValue;
                int expiresIn = int.Parse(jsonData.Properties["expires_in"].StringValue);

                // Extract email from JWT
                string payload = JwtPayloadDecode.GetPayload(jsonData.Properties["id_token"].StringValue);
                var jsonPayload = JsonConv.ParseJson(payload);
                string email = jsonPayload.Properties["email"].StringValue;
                return new LoginResponse()
                {
                    Email = email,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = expiresIn
                };
            }
        }

        public void OpenBrowser(string link)
        {
            try
            {
                Process.Start(link);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open browser\n" + e.Message + "\n" + e.StackTrace);
            }
        }

    }

    public struct LoginResponse
    {
        public string Email;
        public string AccessToken;
        public string RefreshToken;
        public int ExpiresIn;
    }
}
