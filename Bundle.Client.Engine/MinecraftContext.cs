using Bundle.Client.Options;
using Bundle.Client.Palettes;
using Bundle.Runtime;
using Bundle.Client.Session;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client
{
    public sealed class MinecraftContext : IDisposable
    { 
        private readonly IMinecraftClient _client;
        private readonly ILogger _logger;
        public MinecraftContext(IMinecraftClient client, ILogger logger, IOptions<ServerOptions> serverOptions, UserSession userSession)
        {
            _client = client;
            ServerOptions = serverOptions.Value;
            UserSession = userSession;
            _logger = logger;
        }

        public ServerOptions ServerOptions { get; }
        public UserSession UserSession { get; }
        public int CompressionTreshold { get; set; }
        public bool Encrypted { get; set; }
        public PacketTypePalette Palette { get; set; }
        public void SendPacket(Packet packet)
        {
            _client.SendPacket(packet, CompressionTreshold, Encrypted);

            if(Palette != null)
            {
                var type = Palette.GetOutgoingTypeById(packet.Id!.Value);
                _logger.LogTrace($"--> {type}");
            }
            else
            {
                _logger.LogTrace($"--> {packet}");
            }
        }

        public Packet ReadNextPacket()
        {
            return _client.ReadNextPacket(CompressionTreshold, Encrypted);
        }

        public void Disconnect()
        {
            if (!_client.IsConnected())
                return;

            SendPacket(Packet.FromBytes(0x00, Array.Empty<byte>()));
            _client.Disconnect();
        }

        public void Dispose()
        {
            Console.WriteLine("FEFE");
        }
    }
}
