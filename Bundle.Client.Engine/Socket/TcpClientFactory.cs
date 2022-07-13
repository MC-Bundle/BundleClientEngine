using Bundle.Client.Options;
using Bundle.Runtime;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Socket
{
    public class TcpClientFactory : ITcpClientFactory
    {
        private readonly ServerOptions _serverOptions;

        public TcpClientFactory(IOptions<ServerOptions> serverOptions)
        {
            _serverOptions = serverOptions.Value;
        }

        public TcpClient Create()
        {
            var tcpClient = new TcpClient();
            tcpClient.ReceiveBufferSize = 1024 * 1024;
            tcpClient.ReceiveTimeout = 300000;
            tcpClient.SendTimeout = 300000;
            return tcpClient;
        }

        public TcpClient Create(string host, int port)
        {
            var tcpClient = new TcpClient(host, port);
            tcpClient.ReceiveBufferSize = 1024 * 1024;
            tcpClient.ReceiveTimeout = 300000;
            return tcpClient;
        }
    }
}
