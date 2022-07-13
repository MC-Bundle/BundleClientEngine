using Bundle.Client.Json;
using Bundle.Client.Options;
using Bundle.Runtime;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bundle.Client.Authorization
{
    public class XboxLive
    {
        private string userAgent = "Mozilla/5.0 (XboxReplay; XboxLiveAuth/3.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";

        private Regex ppft = new Regex("sFTTag:'.*value=\"(.*)\"\\/>'");
        private Regex urlPost = new Regex("urlPost:'(.+?(?=\'))");
        private Regex confirm = new Regex("identity\\/confirm");
        private Regex invalidAccount = new Regex("Sign in to", RegexOptions.IgnoreCase);
        private Regex twoFA = new Regex("Help us protect your account", RegexOptions.IgnoreCase);

        private readonly ITcpClientFactory _tcpClientFactory;
        private readonly XboxLiveOptions _xboxLiveOptions;
        private readonly ServerOptions _serverOptions;
        public XboxLive(ITcpClientFactory tcpClientFactory, IOptions<XboxLiveOptions> options, IOptions<ServerOptions> serverOptions)
        {
            _tcpClientFactory = tcpClientFactory;
            _xboxLiveOptions = options.Value;
            _serverOptions = serverOptions.Value;
        }
        public string SignInUrl { get { return _xboxLiveOptions.Authorize; } }

        public PreAuthResponse PreAuth()
        {
            var request = new ProxiedWebRequest(_xboxLiveOptions.Authorize, _tcpClientFactory);
            request.UserAgent = userAgent;
            var response = request.Get();

            string html = response.Body;

            string PPFT = ppft.Match(html).Groups[1].Value;
            string urlPost = this.urlPost.Match(html).Groups[1].Value;

            if (string.IsNullOrEmpty(PPFT) || string.IsNullOrEmpty(urlPost))
            {
                throw new Exception("Fail to extract PPFT or urlPost");
            }
            //Console.WriteLine("PPFT: {0}", PPFT);
            //Console.WriteLine();
            //Console.WriteLine("urlPost: {0}", urlPost);

            return new PreAuthResponse()
            {
                UrlPost = urlPost,
                PPFT = PPFT,
                Cookie = response.Cookies
            };
        }

        /// <summary>
        /// Perform login request
        /// </summary>
        /// <remarks>This step is to send the login request by using the PreAuth response</remarks>
        /// <param name="email">Microsoft account email</param>
        /// <param name="password">Account password</param>
        /// <param name="preAuth"></param>
        /// <returns></returns>
        public LoginResponse UserLogin(string email, string password, PreAuthResponse preAuth)
        {
            var request = new ProxiedWebRequest(preAuth.UrlPost, preAuth.Cookie, _tcpClientFactory);
            request.UserAgent = userAgent;

            string postData = "login=" + Uri.EscapeDataString(email)
                 + "&loginfmt=" + Uri.EscapeDataString(email)
                 + "&passwd=" + Uri.EscapeDataString(password)
                 + "&PPFT=" + Uri.EscapeDataString(preAuth.PPFT);

            var response = request.Post("application/x-www-form-urlencoded", postData);

            if (response.StatusCode >= 300 && response.StatusCode <= 399)
            {
                string url = response.Headers.Get("Location");
                string hash = url.Split('#')[1];

                var request2 = new ProxiedWebRequest(url, _tcpClientFactory);
                var response2 = request2.Get();

                if (response2.StatusCode != 200)
                {
                    throw new Exception("Authentication failed");
                }

                if (string.IsNullOrEmpty(hash))
                {
                    throw new Exception("Cannot extract access token");
                }
                var dict = ParseQueryString(hash);

                //foreach (var pair in dict)
                //{
                //    Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
                //}

                return new LoginResponse()
                {
                    Email = email,
                    AccessToken = dict["access_token"],
                    RefreshToken = dict["refresh_token"],
                    ExpiresIn = int.Parse(dict["expires_in"])
                };
            }
            else
            {
                if (twoFA.IsMatch(response.Body))
                {
                    throw new Exception("2FA enabled but not supported yet. Use browser sign-in method or try to disable 2FA in Microsoft account settings");
                }
                else if (invalidAccount.IsMatch(response.Body))
                {
                    throw new Exception("Invalid credentials. Check your credentials");
                }
                else throw new Exception("Unexpected response. Check your credentials. Response code: " + response.StatusCode);
            }
        }

        static public Dictionary<string, string> ParseQueryString(string query)
        {
            return query.Split('&')
                .ToDictionary(c => c.Split('=')[0],
                              c => Uri.UnescapeDataString(c.Split('=')[1]));
        }

        public XblAuthenticateResponse XblAuthenticate(LoginResponse loginResponse)
        {
            var request = new ProxiedWebRequest(_xboxLiveOptions.Xbl, _tcpClientFactory);
            request.UserAgent = userAgent;
            request.Accept = "application/json";
            request.Headers.Add("x-xbl-contract-version", "0");

            var accessToken = loginResponse.AccessToken;
            if (_serverOptions.LoginMethod == "browser")
            {
                // Our own client ID must have d= in front of the token or HTTP status 400
                // "Stolen" client ID must not have d= in front of the token or HTTP status 400
                accessToken = "d=" + accessToken;
            }

            string payload = "{"
                + "\"Properties\": {"
                + "\"AuthMethod\": \"RPS\","
                + "\"SiteName\": \"user.auth.xboxlive.com\","
                + "\"RpsTicket\": \"" + accessToken + "\""
                + "},"
                + "\"RelyingParty\": \"http://auth.xboxlive.com\","
                + "\"TokenType\": \"JWT\""
                + "}";
            var response = request.Post("application/json", payload);
            if (response.StatusCode == 200)
            {
                string jsonString = response.Body;
                //Console.WriteLine(jsonString);

                JsonConv.JSONData json = JsonConv.ParseJson(jsonString);
                string token = json.Properties["Token"].StringValue;
                string userHash = json.Properties["DisplayClaims"].Properties["xui"].DataArray[0].Properties["uhs"].StringValue;
                return new XblAuthenticateResponse()
                {
                    Token = token,
                    UserHash = userHash
                };
            }
            else
            {
                throw new Exception("XBL Authentication failed");
            }
        }

        public XSTSAuthenticateResponse XSTSAuthenticate(XblAuthenticateResponse xblResponse)
        {
            var request = new ProxiedWebRequest(_xboxLiveOptions.Xsts, _tcpClientFactory);
            request.UserAgent = userAgent;
            request.Accept = "application/json";
            request.Headers.Add("x-xbl-contract-version", "1");

            string payload = "{"
                + "\"Properties\": {"
                + "\"SandboxId\": \"RETAIL\","
                + "\"UserTokens\": ["
                + "\"" + xblResponse.Token + "\""
                + "]"
                + "},"
                + "\"RelyingParty\": \"rp://api.minecraftservices.com/\","
                + "\"TokenType\": \"JWT\""
                + "}";
            var response = request.Post("application/json", payload);
            if (response.StatusCode == 200)
            {
                string jsonString = response.Body;
                JsonConv.JSONData json = JsonConv.ParseJson(jsonString);
                string token = json.Properties["Token"].StringValue;
                string userHash = json.Properties["DisplayClaims"].Properties["xui"].DataArray[0].Properties["uhs"].StringValue;
                return new XSTSAuthenticateResponse()
                {
                    Token = token,
                    UserHash = userHash
                };
            }
            else
            {
                if (response.StatusCode == 401)
                {
                    JsonConv.JSONData json = JsonConv.ParseJson(response.Body);
                    if (json.Properties["XErr"].StringValue == "2148916233")
                    {
                        throw new Exception("The account doesn't have an Xbox account");
                    }
                    else if (json.Properties["XErr"].StringValue == "2148916238")
                    {
                        throw new Exception("The account is a child (under 18) and cannot proceed unless the account is added to a Family by an adult");
                    }
                    else throw new Exception("Unknown XSTS error code: " + json.Properties["XErr"].StringValue);
                }
                else
                {
                    throw new Exception("XSTS Authentication failed");
                }
            }
        }

    }

    public struct PreAuthResponse
    {
        public string UrlPost;
        public string PPFT;
        public NameValueCollection Cookie;
    }

    public struct XblAuthenticateResponse
    {
        public string Token;
        public string UserHash;
    }

    public struct XSTSAuthenticateResponse
    {
        public string Token;
        public string UserHash;
    }
}
