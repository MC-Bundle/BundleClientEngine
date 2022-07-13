using Bundle.Client.Helpers;
using Bundle.Client.Json;
using Bundle.Client.Options;
using Bundle.Client.Session;
using Bundle.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;

namespace Bundle.Client.Authorization
{
    public class SignManager : ISignManager
    {
        private readonly MinecraftWithXbox _minecraftWithXbox;
        private readonly Microsoft _microsoft;
        private readonly XboxLive _xboxLive;
        private readonly ServerOptions _serverOptions;
        private readonly ITcpClientFactory _tcpClientFactory;
        private UserSession _userSession;
        private readonly UserSign _userSign;

        public SignManager(
            IServiceScopeFactory serviceScopeFactory,
            IOptions<ServerOptions> serverOptions,
            ITcpClientFactory tcpClientFactory,
            UserSession userSession,
            IOptions<UserSign> userSign)
        {
            var scopeProvider = serviceScopeFactory.CreateScope().ServiceProvider;
            _minecraftWithXbox = scopeProvider.GetRequiredService<MinecraftWithXbox>();
            _microsoft = scopeProvider.GetRequiredService<Microsoft>();
            _xboxLive = scopeProvider.GetRequiredService<XboxLive>();
            _serverOptions = serverOptions.Value;
            _tcpClientFactory = tcpClientFactory;
            _userSession = userSession;
            _userSign = userSign.Value;
        }

        public LoginResult SignIn()
        {
            if (_userSign.AccountType == AccountType.Mojang)
                return SignInMojang(_userSign.Login, _userSign.Password);
            else
                return SignInMicrosoft(_userSign.Login, _userSign.Password);
        }

        public LoginResult SignInMojang(string login, string password)
        {
            LoginResult result = LoginResult.NullError;
            if (SessionCache.Contains(login))
            {
                result = Refrash();
            }

            if (result != LoginResult.Success)
            {
                result = GetLogin(login, password, AccountType.Mojang, out UserSession sessionToken);
                _userSession.SessionId = sessionToken.SessionId;
                _userSession.RefreshToken = sessionToken.RefreshToken;
                _userSession.ClientId = sessionToken.ClientId;
                _userSession.UserName = sessionToken.UserName;
                _userSession.Uuid = sessionToken.Uuid;
            }

            if (result == LoginResult.Success)
            {
                SessionCache.Store(login, _userSession);
            }
            return result;

        }

        public LoginResult SignInMicrosoft(string login, string password)
        {
            LoginResult result = LoginResult.NullError;
            if (SessionCache.Contains(login))
            {
                result = Refrash();
            }
            if (result != LoginResult.Success)
            {
                result = GetLogin(login, password, AccountType.Microsoft, out UserSession sessionToken);
                _userSession.SessionId = sessionToken.SessionId;
                _userSession.RefreshToken = sessionToken.RefreshToken;
                _userSession.ClientId = sessionToken.ClientId;
                _userSession.UserName = sessionToken.UserName;
                _userSession.Uuid = sessionToken.Uuid;
            }

            if (result == LoginResult.Success)
            {
                SessionCache.Store(login, _userSession);
            }
            return result;

        }

        public LoginResult Refrash()
        {
            var result = GetTokenValidation(_userSession);
            if (result != LoginResult.Success && !string.IsNullOrWhiteSpace(_userSession.RefreshToken))
            {
                result = MicrosoftLoginRefresh(_userSession.RefreshToken, out UserSession sessionToken);
                _userSession.SessionId = sessionToken.SessionId;
                _userSession.RefreshToken = sessionToken.RefreshToken;
                _userSession.ClientId = sessionToken.ClientId;
                _userSession.UserName = sessionToken.UserName;
                _userSession.Uuid = sessionToken.Uuid;
            }
            return result;
        }

        private LoginResult GetLogin(string user, string pass, AccountType type, out UserSession session)
        {
            if (type == AccountType.Microsoft)
            {
                if (_serverOptions.LoginMethod == "mcc")
                    return MicrosoftMCCLogin(user, pass, out session);
                else
                    return MicrosoftBrowserLogin(out session, user);
            }
            else if (type == AccountType.Mojang)
            {
                return MojangLogin(user, pass, out session);
            }
            else throw new InvalidOperationException("Account type must be Mojang or Microsoft");
        }

        private LoginResult MojangLogin(string user, string pass, out UserSession session)
        {
            session = new UserSession() { ClientId = Guid.NewGuid().ToString().Replace("-", "") };

            try
            {
                string result = "";
                string json_request = "{\"agent\": { \"name\": \"Minecraft\", \"version\": 1 }, \"username\": \"" + JsonEncode(user) + "\", \"password\": \"" + JsonEncode(pass) + "\", \"clientToken\": \"" + JsonEncode(session.ClientId) + "\" }";
                int code = DoHTTPSPost("authserver.mojang.com", "/authenticate", json_request, ref result);
                if (code == 200)
                {
                    if (result.Contains("availableProfiles\":[]}"))
                    {
                        return LoginResult.NotPremium;
                    }
                    else
                    {
                        JsonConv.JSONData loginResponse = JsonConv.ParseJson(result);
                        if (loginResponse.Properties.ContainsKey("accessToken")
                            && loginResponse.Properties.ContainsKey("selectedProfile")
                            && loginResponse.Properties["selectedProfile"].Properties.ContainsKey("id")
                            && loginResponse.Properties["selectedProfile"].Properties.ContainsKey("name"))
                        {
                            session.SessionId = loginResponse.Properties["accessToken"].StringValue;
                            session.Uuid = loginResponse.Properties["selectedProfile"].Properties["id"].StringValue;
                            session.UserName = loginResponse.Properties["selectedProfile"].Properties["name"].StringValue;
                            return LoginResult.Success;
                        }
                        else return LoginResult.InvalidResponse;
                    }
                }
                else if (code == 403)
                {
                    if (result.Contains("UserMigratedException"))
                    {
                        return LoginResult.AccountMigrated;
                    }
                    else return LoginResult.WrongPassword;
                }
                else if (code == 503)
                {
                    return LoginResult.ServiceUnavailable;
                }
                else
                {
                    Console.WriteLine("лучший текст в мире");
                    return LoginResult.OtherError;
                }
            }
            catch (System.Security.Authentication.AuthenticationException e)
            {
                return LoginResult.SSLError;
            }
            catch (System.IO.IOException e)
            {
                if (e.Message.Contains("authentication"))
                {
                    return LoginResult.SSLError;
                }
                else return LoginResult.OtherError;
            }
            catch (Exception e)
            {
                return LoginResult.OtherError;
            }
        }


        private LoginResult MicrosoftMCCLogin(string email, string password, out UserSession session)
        {
            try
            {
                var msaResponse = _xboxLive.UserLogin(email, password, _xboxLive.PreAuth());
                // Remove refresh token for MCC sign method
                msaResponse.RefreshToken = string.Empty;
                return MicrosoftLogin(msaResponse, out session);
            }
            catch (Exception e)
            {
                session = new UserSession() { ClientId = Guid.NewGuid().ToString().Replace("-", "") };
                Console.WriteLine("§cMicrosoft authenticate failed: " + e.Message);
                return LoginResult.WrongPassword; // Might not always be wrong password
            }
        }

        private LoginResult MicrosoftBrowserLogin(out UserSession session, string loginHint = "")
        {
            if (string.IsNullOrEmpty(loginHint))
                _microsoft.OpenBrowser(_microsoft.SignInUrl);
            else
                _microsoft.OpenBrowser(_microsoft.GetSignInUrlWithHint(loginHint));
            Console.WriteLine("Your browser should open automatically. If not, open the link below in your browser.");
            Console.WriteLine("\n" + _microsoft.SignInUrl + "\n");

            Console.WriteLine("Paste your code here");
            string code = Console.ReadLine();

            var msaResponse = _microsoft.RequestAccessToken(code);
            return MicrosoftLogin(msaResponse, out session);
        }

        private LoginResult MicrosoftLoginRefresh(string refreshToken, out UserSession session)
        {
            var msaResponse = _microsoft.RefreshAccessToken(refreshToken);
            return MicrosoftLogin(msaResponse, out session);
        }


        private LoginResult MicrosoftLogin(LoginResponse msaResponse, out UserSession session)
        {
            session = new UserSession() { ClientId = Guid.NewGuid().ToString().Replace("-", "") };

            try
            {
                var xblResponse = _xboxLive.XblAuthenticate(msaResponse);
                var xsts = _xboxLive.XSTSAuthenticate(xblResponse); // Might throw even password correct

                string accessToken = _minecraftWithXbox.LoginWithXbox(xsts.UserHash, xsts.Token);
                bool hasGame = _minecraftWithXbox.UserHasGame(accessToken);
                if (hasGame)
                {
                    var profile = _minecraftWithXbox.GetUserProfile(accessToken);
                    session.UserName = profile.UserName;
                    session.Uuid = profile.UUID;
                    session.SessionId = accessToken;
                    session.RefreshToken = msaResponse.RefreshToken;
                    return LoginResult.Success;
                }
                else
                {
                    return LoginResult.NotPremium;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("§cMicrosoft authenticate failed: " + e.Message);
                return LoginResult.WrongPassword; // Might not always be wrong password
            }
        }

        private LoginResult GetTokenValidation(UserSession session)
        {
            var payload = JwtPayloadDecode.GetPayload(session.SessionId);
            var json = JsonConv.ParseJson(payload);
            var expTimestamp = long.Parse(json.Properties["exp"].StringValue);
            var now = DateTime.Now;
            var tokenExp = UnixTimeStampToDateTime(expTimestamp);
            if (now < tokenExp)
            {
                // Still valid
                return LoginResult.Success;
            }
            else
            {
                // Token expired
                return LoginResult.LoginRequired;
            }
        }

        private LoginResult GetNewToken(UserSession currentsession, out UserSession session)
        {
            session = new UserSession();
            try
            {
                string result = "";
                string json_request = "{ \"accessToken\": \"" + JsonEncode(currentsession.SessionId) + "\", \"clientToken\": \"" + JsonEncode(currentsession.ClientId) + "\", \"selectedProfile\": { \"id\": \"" + JsonEncode(currentsession.Uuid) + "\", \"name\": \"" + JsonEncode(currentsession.UserName) + "\" } }";
                int code = DoHTTPSPost("authserver.mojang.com", "/refresh", json_request, ref result);
                if (code == 200)
                {
                    if (result == null)
                    {
                        return LoginResult.NullError;
                    }
                    else
                    {
                        JsonConv.JSONData loginResponse = JsonConv.ParseJson(result);
                        if (loginResponse.Properties.ContainsKey("accessToken")
                            && loginResponse.Properties.ContainsKey("selectedProfile")
                            && loginResponse.Properties["selectedProfile"].Properties.ContainsKey("id")
                            && loginResponse.Properties["selectedProfile"].Properties.ContainsKey("name"))
                        {
                            session.SessionId = loginResponse.Properties["accessToken"].StringValue;
                            session.Uuid = loginResponse.Properties["selectedProfile"].Properties["id"].StringValue;
                            session.UserName = loginResponse.Properties["selectedProfile"].Properties["name"].StringValue;
                            return LoginResult.Success;
                        }
                        else return LoginResult.InvalidResponse;
                    }
                }
                else if (code == 403 && result.Contains("InvalidToken"))
                {
                    return LoginResult.InvalidToken;
                }
                else
                {
                    Console.WriteLine("лучший текст в мире");
                    return LoginResult.OtherError;
                }
            }
            catch
            {
                return LoginResult.OtherError;
            }
        }

        public bool SessionCheck(string uuid, string accesstoken, string serverhash)
        {
            try
            {
                string result = "";
                string json_request = "{\"accessToken\":\"" + accesstoken + "\",\"selectedProfile\":\"" + uuid + "\",\"serverId\":\"" + serverhash + "\"}";
                int code = DoHTTPSPost("sessionserver.mojang.com", "/session/minecraft/join", json_request, ref result);
                return (code >= 200 && code < 300);
            }
            catch { return false; }
        }

        private int DoHTTPSGet(string host, string endpoint, string cookies, ref string result)
        {
            List<String> http_request = new List<string>();
            http_request.Add("GET " + endpoint + " HTTP/1.1");
            http_request.Add("Cookie: " + cookies);
            http_request.Add("Cache-Control: no-cache");
            http_request.Add("Pragma: no-cache");
            http_request.Add("Host: " + host);
            http_request.Add("User-Agent: Java/1.6.0_27");
            http_request.Add("Accept-Charset: ISO-8859-1,UTF-8;q=0.7,*;q=0.7");
            http_request.Add("Connection: close");
            http_request.Add("");
            http_request.Add("");
            return DoHTTPSRequest(http_request, host, ref result);
        }

        private int DoHTTPSPost(string host, string endpoint, string request, ref string result)
        {
            List<String> http_request = new List<string>();
            http_request.Add("POST " + endpoint + " HTTP/1.1");
            http_request.Add("Host: " + host);
            http_request.Add("User-Agent: MCC/0");
            http_request.Add("Content-Type: application/json");
            http_request.Add("Content-Length: " + Encoding.ASCII.GetBytes(request).Length);
            http_request.Add("Connection: close");
            http_request.Add("");
            http_request.Add(request);
            return DoHTTPSRequest(http_request, host, ref result);
        }

        private int DoHTTPSRequest(List<string> headers, string host, ref string result)
        {
            string postResult = null;
            int statusCode = 520;
            Exception exception = null;
            AutoTimeout.Perform(() =>
            {
                try
                {
                    TcpClient client = _tcpClientFactory.Create(host, 443);
                    SslStream stream = new SslStream(client.GetStream());
                    stream.AuthenticateAsClient(host, null, (SslProtocols)3072, true); // Enable TLS 1.2. Hotfix for #1780

                    stream.Write(Encoding.ASCII.GetBytes(String.Join("\r\n", headers.ToArray())));
                    System.IO.StreamReader sr = new System.IO.StreamReader(stream);
                    string raw_result = sr.ReadToEnd();

                    if (raw_result.StartsWith("HTTP/1.1"))
                    {
                        postResult = raw_result.Substring(raw_result.IndexOf("\r\n\r\n") + 4);
                        statusCode = ConvertHelper.StringToInt(raw_result.Split(' ')[1]);
                    }
                    else statusCode = 520; //Web server is returning an unknown error
                }
                catch (Exception e)
                {
                    if (!(e is System.Threading.ThreadAbortException))
                    {
                        exception = e;
                    }
                }
            }, TimeSpan.FromSeconds(30));
            result = postResult;
            if (exception != null)
                throw exception;
            return statusCode;
        }

        private string JsonEncode(string text)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in text)
            {
                if ((c >= '0' && c <= '9') ||
                    (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z'))
                {
                    result.Append(c);
                }
                else
                {
                    result.AppendFormat(@"\u{0:x4}", (int)c);
                }
            }

            return result.ToString();
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }

    public enum LoginResult { OtherError, ServiceUnavailable, SSLError, Success, WrongPassword, AccountMigrated, NotPremium, LoginRequired, InvalidToken, InvalidResponse, NullError, UserCancel };
    public enum AccountType { Mojang, Microsoft };
}
