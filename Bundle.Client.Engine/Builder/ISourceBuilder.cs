using Bundle.Client.Routing;
using Bundle.Client.Sources;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bundle.Client.Builder
{
    public interface ISourceBuilder
    {
        public IServiceProvider Services { get; }
        public ISourceBuilder New<T>(string defualtRoute = null) where T : ISourceHandler;

        public Task<ISourceBuilder> Run<T>() where T : ISourceHandler;
        public Task<ISourceBuilder> Run<T>(object value) where T : ISourceHandler;
        public Task<ISourceBuilder> Run<T>(string route) where T : ISourceHandler;
        public Task<ISourceBuilder> Run<T>(string route, object value) where T : ISourceHandler;

        public Task<ISourceBuilder> Start<T>(CancellationToken token) where T : ISourceHandler;
        public Task<ISourceBuilder> Start<T>(string route, CancellationToken token) where T : ISourceHandler;

        public ISourceBuilder Add(RouteEndpoint routeEndpoint);
    }
}
