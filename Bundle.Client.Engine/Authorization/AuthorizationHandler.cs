using Bundle.Client.Pipline;
using Bundle.Client.Sources;
using Bundle.Runtime;
using Bundle.Runtime.Crypto;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Bundle.Client.Authorization
{
    public class AuthorizationHandler : IAuthorizationHandler
    {
        private readonly IMinecraftClient _minecraftClient;
        private readonly MinecraftContext _minecraftContext;
        private readonly IPipelineApplication _pipelineApplication;
        private readonly ISignManager _signManager;

        public AuthorizationHandler(IMinecraftClient minecraftClient, MinecraftContext minecraftContext, IPipelineApplication pipelineApplication, ISignManager signManager)
        {
            _minecraftClient = minecraftClient;
            _minecraftContext = minecraftContext;
            _pipelineApplication = pipelineApplication;
            _signManager = signManager;
        }

        public bool SignIn(CancellationToken cToken)
        {
            byte[] protocol_version = ByteHelper.GetVarInt(GlobalProtocolVersion.Value);
            string server_address = _minecraftContext.ServerOptions.ServerIP;
            byte[] server_port = ByteHelper.GetUShort((ushort)_minecraftContext.ServerOptions.ServerPort);
            byte[] next_state = ByteHelper.GetVarInt(2);
            byte[] handshake_packet = ByteHelper.ConcatBytes(protocol_version, ByteHelper.GetString(server_address), server_port, next_state);
            //mc.or1k.net
            //while (true)
            //{
            //    _minecraftContext.SendPacket(Packet.FromBytes(0x00, handshake_packet));
            //}
            //_minecraftContext.SendPacket(Packet.FromBytes(0x00, handshake_packet));

            _minecraftContext.SendPacket(Packet.FromBytes(0x00, handshake_packet));

            byte[] login_packet = ByteHelper.GetString(_minecraftContext.UserSession.UserName);

            _minecraftContext.SendPacket(Packet.FromBytes(0x00, login_packet));

            while (true)
            {
                var packet = _minecraftContext.ReadNextPacket();
                if (packet.Id == 0x00) //Login rejected
                {
                    var ssr = packet.ReadNextString();
                    //_errorMessageProvider.Execute(ChatParser.ParseText(packet.ReadNextString()));
                    return false;
                }
                else if (packet.Id == 0x01) //Encryption request
                {
                    string serverID = packet.ReadNextString();
                    byte[] Serverkey = packet.ReadNextByteArray();
                    byte[] token = packet.ReadNextByteArray();
                    return StartEncryption(token, serverID, Serverkey, cToken);
                }
                else if (packet.Id == 0x02) //Login successful
                {
                    _pipelineApplication.Start<MinecraftClientSource>(cToken);
                    return true;
                }
                else HandleConnectionPacket(packet);
            }
        }

        private bool StartEncryption(byte[] token, string serverIDhash, byte[] serverKey, CancellationToken сtoken)
        {
            var uuid = _minecraftContext.UserSession.Uuid;
            var sessionID = _minecraftContext.UserSession.SessionId;
            System.Security.Cryptography.RSACryptoServiceProvider RSAService = CryptoHandler.DecodeRSAPublicKey(serverKey);
            byte[] secretKey = CryptoHandler.GenerateAESPrivateKey();

            if (serverIDhash != "-")
            {
                if (!_signManager.SessionCheck(uuid, sessionID, CryptoHandler.getServerHash(serverIDhash, serverKey, secretKey)))
                {
                    _minecraftClient.Disconnect();
                    return false;
                }
            }

            //Encrypt the data
            byte[] key_enc = ByteHelper.GetArray(RSAService.Encrypt(secretKey, false));
            byte[] token_enc = ByteHelper.GetArray(RSAService.Encrypt(token, false));

            //Encryption Response packet
            _minecraftContext.SendPacket(Packet.FromBytes(0x01, ByteHelper.ConcatBytes(key_enc, token_enc)));

            _minecraftClient.Scrambler = CryptoHandler.getAesStream(_minecraftClient.GetStream(), secretKey);
            _minecraftContext.Encrypted = true;

            //Process the next packet
            int loopPrevention = UInt16.MaxValue;
            while (true)
            {
                var packet = _minecraftContext.ReadNextPacket();
                if (packet.Id < 0 || loopPrevention-- < 0) // Failed to read packet or too many iterations (issue #1150)
                {
                    return false;
                }
                else if (packet.Id == 0x00) //Login rejected
                {
                    //_errorMessageProvider.Execute(ChatParser.ParseText(packet.ReadNextString()));
                    return false;
                }
                else if (packet.Id == 0x02) //Login successful
                {
                    _pipelineApplication.Start<MinecraftClientSource>(сtoken);
                    return true;
                }
                else HandleConnectionPacket(packet);
            }
        }

        private void HandleConnectionPacket(Packet packet)
        {
            try
            {
                switch (packet.Id)
                {
                    case 0x03:
                        if (GlobalProtocolVersion.Value >= MinecraftVersion.MC18Version)
                            _minecraftContext.CompressionTreshold = packet.ReadNextVarInt();

                        _pipelineApplication.Run<MinecraftClientSource>(packet).Wait();
                        return;
                    case 0x04:
                        int messageId = packet.ReadNextVarInt();
                        string channel = packet.ReadNextString();
                        List<byte> responseData = new List<byte>();
                        bool understood = true;
                        SendLoginPluginResponse(messageId, understood, responseData.ToArray());
                        return;
                    default:
                        return;
                }
            }
            catch (Exception innerException)
            {
                if (innerException is ThreadAbortException || innerException is SocketException || innerException.InnerException is SocketException)
                    throw;
                throw new System.IO.InvalidDataException(
                    "exception.packet_process",
                    innerException);
            }
        }

        private bool SendLoginPluginResponse(int messageId, bool understood, byte[] data)
        {
            try
            {
                _minecraftContext.SendPacket(Packet.FromBytes(0x02, ByteHelper.ConcatBytes(ByteHelper.GetVarInt(messageId), ByteHelper.GetBool(understood), data)));
                return true;
            }
            catch (SocketException) { return false; }
            catch (System.IO.IOException) { return false; }
            catch (ObjectDisposedException) { return false; }
        }
    }
}
