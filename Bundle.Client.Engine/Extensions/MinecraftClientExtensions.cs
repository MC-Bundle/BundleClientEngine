using Bundle.Client.Authorization;
using Bundle.Client.Builder;
using Bundle.Client.Helpers;
using Bundle.Client.Options;
using Bundle.Client.Palettes;
using Bundle.Client.Routing;
using Bundle.Client.Session;
using Bundle.Client.Sources;
using Bundle.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;

namespace Bundle.Client.Extensions
{
    public static class MinecraftClientExtensions
    {
        public static ISourceBuilder NewMinecraftClient(this ISourceBuilder sourceBuilder)
        {
            sourceBuilder.New<MinecraftClientSource>();

            var logger = sourceBuilder.Services.GetRequiredService<ILogger>();
            var minecraftClient = sourceBuilder.Services.GetRequiredService<IMinecraftClient>();
            sourceBuilder.Use<Packet>(p =>
            {

                if (p.MinecraftContext.Palette != null)
                {
                    var inType = p.MinecraftContext.Palette.GetIncommingTypeById(p.Value.Id!.Value);
                    var data = new RequestClientData();
                    data.PacketType = inType;
                    p.RequestData = data;
                    logger.LogTrace($"<-- {inType}");
                    return;
                }

                logger.LogTrace($"<-- {p.Value}");

                p.End(Routing.EndStatus.Ok);
            });

            sourceBuilder.Use<Packet>(p =>
            {
                if (p.RequestData.PacketType == PacketTypeIn.Disconnect)
                {

                    var str = p.Value.ReadNextString();
                    var text = ChatParser.ParseText(str);
                    logger.LogCritical(text);
                    minecraftClient.Disconnect();
                }
                else if (p.RequestData.PacketType == PacketTypeIn.KeepAlive)
                {
                    var outPacket = Packet.FromBytes((int)PacketTypeOut.KeepAlive, p.Value.ToArray());
                    p.MinecraftContext.SendPacket(outPacket);
                }
            });
            return sourceBuilder;
        }
        public static MinecraftApplication StartMinecraftClient(this MinecraftApplication app, CancellationToken? cToken = null)
        {
            if (cToken == null)
            {
                var source = new CancellationTokenSource();
                cToken = source.Token;
            }
            var tcpFactory = app.Services.GetRequiredService<ITcpClientFactory>();
            var serverOptions = app.Services.GetRequiredService<IOptions<ServerOptions>>().Value;
            var userSign = app.Services.GetRequiredService<IOptions<UserSign>>().Value;
            var signManager = app.Services.GetRequiredService<ISignManager>();
            var session = app.Services.GetRequiredService<UserSession>();

            LoginResult result;

            if (userSign.Password == null)
            {
                result = LoginResult.Success;
                session.Uuid = "0";
                session.UserName = userSign.Login;

                //SessionCache.Store(session.UserName, session);
            }
            else
            {
                result = signManager.SignIn();
            }

            if (result != LoginResult.Success)
                throw new Exception("Не удалось авторизоваться");


            int protocolVersionValue = 0;

            using (var clientDoPing = new MinecraftClient(app.Services.GetRequiredService<MinecraftConnect>(), tcpFactory, app.Services.GetRequiredService<ICompressor>()))
            {
                clientDoPing.Connect();
                if (ServerHelper.doPing(clientDoPing, serverOptions.ServerIP, serverOptions.ServerPort, ref protocolVersionValue) == false)
                {
                    throw new Exception("Не удалось получить версию протокола");
                }
            }

            GlobalProtocolVersion.Value = protocolVersionValue;

            app.Services.GetRequiredService<MinecraftContext>().Palette = app.Services.GetRequiredService<PacketTypePalette>();
            var client = app.Services.GetRequiredService<IMinecraftClient>();

            if (GlobalProtocolVersion.Value != 0)
            {
                client.Connect();
            }
            else
            {
                throw new Exception("Не удалось получить версию протокола");
            }

            if (!app.Services.GetRequiredService<IAuthorizationHandler>().SignIn(cToken.Value))
            {
                throw new Exception("Не удалось авторизоваться");
            };

            return app;
        }

        public static ISourceBuilder Map(this ISourceBuilder sourceBuilder, PacketTypeIn packetType, RequestDelegate<Packet> requestDelegate)
        {
            sourceBuilder.Use<Packet>(p =>
            {
                if (p.RequestData.PacketType == packetType)
                {
                    requestDelegate(p);
                }
            });

            return sourceBuilder;
        }

        public static ISourceBuilder Map(this ISourceBuilder sourceBuilder, PacketTypeIn packetType, int workerCount, RequestDelegate<Packet> requestDelegate)
        {
            sourceBuilder.Use<Packet>(p =>
            {
                if (p.RequestData.PacketType == packetType)
                {
                    requestDelegate(p);
                }
            });

            return sourceBuilder;
        }
    }
}
