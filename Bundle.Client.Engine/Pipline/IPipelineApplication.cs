using Bundle.Client.Builder;
using Bundle.Client.Routing;
using Bundle.Client.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bundle.Client.Pipline
{
    public interface IPipelineApplication
    {
        public void New(ISourceHandler sourceHandler, string defualtRoute = null);
        public void Add(Type sourceHandlerType, RouteEndpoint routeEndpoint);
        public Task Run<T>() where T : ISourceHandler;
        public Task Run<T>(object value) where T : ISourceHandler;
        public Task Run<T>(string route) where T : ISourceHandler;
        public Task Run<T>(string route, object value) where T : ISourceHandler;

        public Task Start<T>(CancellationToken token) where T : ISourceHandler;
        public Task Start<T>(string route, CancellationToken token) where T : ISourceHandler;
        public Task StopAll();
    }
}
