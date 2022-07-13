using Bundle.Client.Builder;
using Bundle.Runtime;
using Bundle.Client.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bundle.Client.Extensions
{
    public static class TimeoutDetectorExtensions
    {
        public static MinecraftApplication UseTimeoutDetector(this MinecraftApplication app)
        {
            app.New<TimeoutDetectorSource>("/timeout");
            app.UseTrigger("/timeout", p =>
            {
                app.Services.GetRequiredService<IMinecraftClient>().Disconnect();
                app.Services.GetRequiredService<TimeoutDetectorSource>().Stop();
                p.Next("/timeout_message");
            });
            app.UseTrigger("/timeout_message", p =>
            {
                app.Logger.Log(LogLevel.Critical, "timeout error");
            });
            return app;
        }

        public static MinecraftApplication StartTimeoutDetector(this MinecraftApplication app, CancellationToken? token = null)
        {
            if (token == null)
            {
                var source = new CancellationTokenSource();
                token = source.Token;
            }

            app.Start<TimeoutDetectorSource>("/timeout", token.Value).Wait();
            return app;
        }
    }
}
