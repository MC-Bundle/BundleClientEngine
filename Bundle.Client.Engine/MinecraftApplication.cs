using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bundle.Client.Builder;
using Bundle.Client.Routing;
using Bundle.Client.Sources;
using Microsoft.Extensions.Logging;
using Bundle.Client.Pipline;
using System.Threading;

namespace Bundle.Client
{
    public class MinecraftApplication : IApplicationBuilder, ISourceBuilder
    {
        private Type _currentSourceType;
        public MinecraftApplication(IServiceProvider services)
        {
            Services = services.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider;
        }
        public IServiceProvider Services { get; }

        //ToDo
        //public IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();

        public ILogger Logger => Services.GetRequiredService<ILogger>();


        public IPipelineApplication PipelineApplication => Services.GetRequiredService<IPipelineApplication>();
        public static MinecraftApplicationBuilder CreateBuilder(Action<ConfigurationBuilder> action = null)
        {
            var configBuilder = new ConfigurationBuilder();
            if(action != null)
                action(configBuilder);

            var config = configBuilder.Build();
            return new MinecraftApplicationBuilder(config);
        }

        public ISourceBuilder Add(RouteEndpoint routeEndpoint)
        {
            if (_currentSourceType == null)
                throw new Exception();

            PipelineApplication.Add(_currentSourceType, routeEndpoint);
            return this;
        }

        public ISourceBuilder New<T>(string defualtRoute = null) where T : ISourceHandler
        {
            _currentSourceType = typeof(T);
            var source = Services.GetRequiredService<T>();


            PipelineApplication.New(source, defualtRoute);

            return this;
        }

        public async Task<ISourceBuilder> Run<T>() where T : ISourceHandler
        {
            await PipelineApplication.Run<T>();
            return this;
        }
        public async Task<ISourceBuilder> Run<T>(object value) where T : ISourceHandler
        {
            await PipelineApplication.Run<T>(value);
            return this;
        }
        public async Task<ISourceBuilder> Run<T>(string route) where T : ISourceHandler
        {
            await PipelineApplication.Run<T>(route);
            return this;
        }
        public async Task<ISourceBuilder> Run<T>(string route, object value) where T : ISourceHandler
        {
            await PipelineApplication.Run<T>(route, value);
            return this;
        }

        public async Task<ISourceBuilder> Start<T>(CancellationToken token) where T : ISourceHandler
        {
            await PipelineApplication.Start<T>(token);
            return this;
        }

        public async Task<ISourceBuilder> Start<T>(string route, CancellationToken token) where T : ISourceHandler
        {
            await PipelineApplication.Start<T>(route, token);
            return this;
        }

        public async Task<ISourceBuilder> StopAll()
        {
            await PipelineApplication.StopAll();
            return this;
        }

    }
}
