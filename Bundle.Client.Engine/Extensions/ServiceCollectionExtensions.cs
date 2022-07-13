using Bundle.Client.Authorization;
using Bundle.Client.Logger;
using Bundle.Client.Options;
using Bundle.Client.Palettes;
using Bundle.Client.Pipline;
using Bundle.Client.Routing;
using Bundle.Runtime;
using Bundle.Client.Session;
using Bundle.Client.Socket;
using Bundle.Client.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddDefualt(this IServiceCollection services)
        {
            services.Configure<ServerOptions>(p =>
            {
                p.LoginMethod = "mcc";
                p.ServerPort = 25565;
            });

            services.AddSingleton<MinecraftConnect>(p =>
            {
                var connect = new MinecraftConnect();
                var serverOptions = p.GetRequiredService<IOptions<ServerOptions>>().Value;
                connect.Host = serverOptions.ServerIP;
                connect.Port = serverOptions.ServerPort;
                return connect;
            });

            services.Configure<UserSign>(p =>
            {
            });

            var clientId = "54473e32-df8f-42e9-a649-9419b0dab9d3";
            services.Configure<MicrosoftOptions>(p =>
            {
                p.ClientId = clientId;
                p.SigninUrl = string.Format("https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize?client_id={0}&response_type=code&redirect_uri=https%3A%2F%2Fmccteam.github.io%2Fredirect.html&scope=XboxLive.signin%20offline_access%20openid%20email&prompt=select_account&response_mode=fragment", clientId);
                p.TokenUrl = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
            });

            services.Configure<XboxLiveOptions>(p =>
            {
                p.Authorize = "https://login.live.com/oauth20_authorize.srf?client_id=000000004C12AE6F&redirect_uri=https://login.live.com/oauth20_desktop.srf&scope=service::user.auth.xboxlive.com::MBI_SSL&display=touch&response_type=token&locale=en";
                p.Xbl = "https://user.auth.xboxlive.com/user/authenticate";
                p.Xsts = "https://xsts.auth.xboxlive.com/xsts/authorize";
            });

            services.Configure<MinecraftWithXboxOptions>(p =>
            {
                p.LoginWithXbox = "https://api.minecraftservices.com/authentication/login_with_xbox";
                p.Ownership = "https://api.minecraftservices.com/entitlements/mcstore";
                p.Profile = "https://api.minecraftservices.com/minecraft/profile";
            });

            services.AddSingleton<ICompressor, ZlibCompressor>();
            services.AddScoped<MinecraftWithXbox>();
            services.AddScoped<Authorization.Microsoft>();
            services.AddScoped<XboxLive>();
            services.AddScoped<ITcpClientFactory, TcpClientFactory>();
            services.AddSingleton<UserSession>(p =>
            {
                var signUser = p.GetRequiredService<IOptions<UserSign>>();

                return UserSession.FromCache(signUser.Value.Login);
            });

            services.AddLogging(configure =>
            {
                configure.AddMineConsole();
                configure.SetMinimumLevel(LogLevel.Information);
            });

            services.AddScoped<ILogger>(p => p.GetService<ILoggerFactory>().CreateLogger("root"));

            services.AddScoped<ISignManager, SignManager>();
            services.AddScoped<IMinecraftClient, MinecraftClient>();
            services.AddScoped<IAuthorizationHandler, AuthorizationHandler>();
            services.AddScoped<TimeoutDetectorSource>();
            services.AddScoped<MinecraftClientSource>();
            services.AddScoped<MinecraftContext>();
            services.AddScoped<IPipelineApplication, PipelineApplication>();
            services.AddScoped<PacketTypePalette>(p => PaletteHelper.GetTypeHandler());
            return services;
        }
    }
}
