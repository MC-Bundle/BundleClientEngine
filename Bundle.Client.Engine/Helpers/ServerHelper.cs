using Bundle.Client.Json;
using Bundle.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Helpers
{
    public static class ServerHelper
    {
        public static bool doPing(IMinecraftClient client, string host, int port, ref int protocolversion)
        {
            string version = "";
            //DataTypes dataTypes = new DataTypes(new ProtocolVersion() { Value = MinecraftVersion.MC18Version });

            byte[] packet_id = ByteHelper.GetVarInt(0);
            byte[] protocol_version = ByteHelper.GetVarInt(-1);
            byte[] server_port = BitConverter.GetBytes((ushort)port); Array.Reverse(server_port);
            byte[] next_state = ByteHelper.GetVarInt(1);
            byte[] packet = ByteHelper.ConcatBytes(packet_id, protocol_version, ByteHelper.GetString(host), server_port, next_state);
            byte[] tosend = ByteHelper.ConcatBytes(ByteHelper.GetVarInt(packet.Length), packet);

            client.SendDataRAW(tosend, false);

            byte[] status_request = ByteHelper.GetVarInt(0);
            byte[] request_packet = ByteHelper.ConcatBytes(ByteHelper.GetVarInt(status_request.Length), status_request);

            client.SendDataRAW(request_packet, false);

            int packetLength = client.ReadNextVarIntRAW(false);
            if (packetLength > 0) //Read Response length
            {
                Packet packetRuntime = Packet.FromBytes(0, client.ReadDataRAW(packetLength, false));
                if (packetRuntime.ReadNextVarInt() == 0x00) //Read Packet ID
                {
                    string result = packetRuntime.ReadNextString(); //Get the Json data


                    if (!String.IsNullOrEmpty(result) && result.StartsWith("{") && result.EndsWith("}"))
                    {
                        JsonConv.JSONData jsonData = JsonConv.ParseJson(result);
                        if (jsonData.Type == JsonConv.JSONData.DataType.Object && jsonData.Properties.ContainsKey("version"))
                        {
                            JsonConv.JSONData versionData = jsonData.Properties["version"];

                            //Retrieve display name of the Minecraft version
                            if (versionData.Properties.ContainsKey("name"))
                                version = versionData.Properties["name"].StringValue;

                            //Retrieve protocol version number for handling this server
                            if (versionData.Properties.ContainsKey("protocol"))
                                protocolversion = int.Parse(versionData.Properties["protocol"].StringValue);


                            return true;
                        }
                    }
                }
            }
            return true;
        }

        public static int GetMaxChatMessageLength()
        {
            return GlobalProtocolVersion.Value > MinecraftVersion.MC110Version
                ? 256
                : 100;
        }
    }
}
