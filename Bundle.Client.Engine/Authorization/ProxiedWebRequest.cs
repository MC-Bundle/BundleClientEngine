using Bundle.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Authorization
{
    public class ProxiedWebRequest
    {
        private readonly string httpVersion = "HTTP/1.0"; // Use 1.0 here because 1.1 server may send chunked data

        private Uri uri;
        private string host { get { return uri.Host; } }
        private int port { get { return uri.Port; } }
        private string path { get { return uri.PathAndQuery; } }
        private bool isSecure { get { return uri.Scheme == "https"; } }

        public NameValueCollection Headers = new NameValueCollection();

        public string UserAgent { get { return Headers.Get("User-Agent"); } set { Headers.Set("User-Agent", value); } }
        public string Accept { get { return Headers.Get("Accept"); } set { Headers.Set("Accept", value); } }
        public string Cookie { set { Headers.Set("Cookie", value); } }


        private ITcpClientFactory _tcpClientFactory;
        public ProxiedWebRequest(string url, ITcpClientFactory tcpClientFactory)
        {
            uri = new Uri(url);
            _tcpClientFactory = tcpClientFactory;
            SetupBasicHeaders();
        }


        public ProxiedWebRequest(string url, NameValueCollection cookies, ITcpClientFactory tcpClientFactory)
        {
            uri = new Uri(url);
            Headers.Add("Cookie", GetCookieString(cookies));
            SetupBasicHeaders();
            _tcpClientFactory = tcpClientFactory;
        }

        private void SetupBasicHeaders()
        {
            Headers.Add("Host", host);
            Headers.Add("User-Agent", "MCC/0");
            Headers.Add("Accept", "*/*");
            Headers.Add("Connection", "close");
        }

        public Response Get()
        {
            return Send("GET");
        }

        public Response Post(string contentType, string body)
        {
            Headers.Add("Content-Type", contentType);
            // Calculate length
            Headers.Add("Content-Length", Encoding.UTF8.GetBytes(body).Length.ToString());
            return Send("POST", body);
        }

        private Response Send(string method, string body = "")
        {
            List<string> requestMessage = new List<string>()
            {
                string.Format("{0} {1} {2}", method.ToUpper(), path, httpVersion) // Request line
            };
            foreach (string key in Headers) // Headers
            {
                var value = Headers[key];
                requestMessage.Add(string.Format("{0}: {1}", key, value));
            }
            requestMessage.Add(""); // <CR><LF>
            if (body != "")
            {
                requestMessage.Add(body);
            }
            else requestMessage.Add(""); // <CR><LF>
            Response response = Response.Empty();
            AutoTimeout.Perform(() =>
            {
                TcpClient client = _tcpClientFactory.Create(host, port);
                Stream stream;
                if (isSecure)
                {
                    stream = new SslStream(client.GetStream());
                    ((SslStream)stream).AuthenticateAsClient(host, null, (SslProtocols)3072, true); // Enable TLS 1.2. Hotfix for #1774
                }
                else
                {
                    stream = client.GetStream();
                }
                string h = string.Join("\r\n", requestMessage.ToArray());
                byte[] data = Encoding.ASCII.GetBytes(h);
                stream.Write(data, 0, data.Length);
                stream.Flush();
                StreamReader sr = new StreamReader(stream);
                string rawResult = sr.ReadToEnd();
                response = ParseResponse(rawResult);
                try
                {
                    sr.Close();
                    stream.Close();
                    client.Close();
                }
                catch { }
            },
            TimeSpan.FromSeconds(30));
            return response;
        }

        private Response ParseResponse(string raw)
        {
            int statusCode;
            string responseBody = "";
            NameValueCollection headers = new NameValueCollection();
            NameValueCollection cookies = new NameValueCollection();
            if (raw.StartsWith("HTTP/1.1") || raw.StartsWith("HTTP/1.0"))
            {
                Queue<string> msg = new Queue<string>(raw.Split(new string[] { "\r\n" }, StringSplitOptions.None));
                statusCode = int.Parse(msg.Dequeue().Split(' ')[1]);

                while (msg.Peek() != "")
                {
                    string[] header = msg.Dequeue().Split(new char[] { ':' }, 2); // Split first ':' only
                    string key = header[0].ToLower(); // Key is case-insensitive
                    string value = header[1];
                    if (key == "set-cookie")
                    {
                        string[] cookie = value.Split(';'); // cookie options are ignored
                        string[] tmp = cookie[0].Split(new char[] { '=' }, 2); // Split first '=' only
                        string cname = tmp[0].Trim();
                        string cvalue = tmp[1].Trim();
                        cookies.Add(cname, cvalue);
                    }
                    else
                    {
                        headers.Add(key, value.Trim());
                    }
                }
                msg.Dequeue();
                if (msg.Count > 0)
                    responseBody = msg.Dequeue();

                return new Response()
                {
                    StatusCode = statusCode,
                    Body = responseBody,
                    Headers = headers,
                    Cookies = cookies
                };
            }
            else
            {
                return new Response()
                {
                    StatusCode = 520, // 502 - Web Server Returned an Unknown Error
                    Body = "",
                    Headers = headers,
                    Cookies = cookies
                };
            }
        }

        private static string GetCookieString(NameValueCollection cookies)
        {
            var sb = new StringBuilder();
            foreach (string key in cookies)
            {
                var value = cookies[key];
                sb.Append(string.Format("{0}={1}; ", key, value));
            }
            string result = sb.ToString();
            return result.Remove(result.Length - 2); // Remove "; " at the end
        }

        public class Response
        {
            public int StatusCode;
            public string Body;
            public NameValueCollection Headers;
            public NameValueCollection Cookies;

            public static Response Empty()
            {
                return new Response()
                {
                    StatusCode = 204, // 204 - No content
                    Body = "",
                    Headers = new NameValueCollection(),
                    Cookies = new NameValueCollection()
                };
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine("Status code: " + StatusCode);
                sb.AppendLine("Headers:");
                foreach (string key in Headers)
                {
                    sb.AppendLine(string.Format("  {0}: {1}", key, Headers[key]));
                }
                if (Cookies.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Cookies: ");
                    foreach (string key in Cookies)
                    {
                        sb.AppendLine(string.Format("  {0}={1}", key, Cookies[key]));
                    }
                }
                if (Body != "")
                {
                    sb.AppendLine();
                    if (Body.Length > 200)
                    {
                        sb.AppendLine("Body: (Truncated to 200 characters)");
                    }
                    else sb.AppendLine("Body: ");
                    sb.AppendLine(Body.Length > 200 ? Body.Substring(0, 200) + "..." : Body);
                }
                return sb.ToString();
            }
        }
    }
}
