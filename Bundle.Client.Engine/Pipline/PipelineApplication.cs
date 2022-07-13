using Bundle.Client.Builder;
using Bundle.Client.Routing;
using Bundle.Client.Sources;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bundle.Client.Pipline
{
    public class PipelineApplication : IPipelineApplication
    {
        private readonly MinecraftContext _minecraftContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly Dictionary<Type, PiplineSource> _piplineSources = new Dictionary<Type, PiplineSource>();

        public PipelineApplication(MinecraftContext minecraftContext, IServiceScopeFactory serviceScopeFactory)
        {
            _minecraftContext = minecraftContext;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void Add(Type sourceHandlerType, RouteEndpoint routeEndpoint)
        {
            _piplineSources[sourceHandlerType].Endpoints.Add(routeEndpoint);
        }

        public void New(ISourceHandler sourceHandler, string defualtRoute = null)
        {
            _piplineSources.Add(sourceHandler.GetType(), new PiplineSource
            {
                SourceHandler = sourceHandler,
                Route = defualtRoute,
                Endpoints = new List<RouteEndpoint>()
            });;
        }

        public async Task Run<T>() where T : ISourceHandler
        {
            var source = _piplineSources[typeof(T)];
            source.Memory();
            RequestContext<object> requestContext = new RequestContext<object>(this, _minecraftContext);
            requestContext.ServiceProvider = _serviceScopeFactory.CreateScope().ServiceProvider;
            foreach (var endpoint in source.NotRouteEndpoints)
            {
                await endpoint.RequestDelegate.Invoke(requestContext);

                if(requestContext.NextRoute != null)
                {
                    requestContext.NextRoute = null;
                    await Run<T>(requestContext.NextRoute);
                    return;
                }
                
                if(requestContext.EndData != null)
                {
                    //Обработать окончание запроса
                    return;
                }
            }
        }

        public async Task Run<T>(object value) where T : ISourceHandler
        {
            var source = _piplineSources[typeof(T)];
            source.Memory();
            RequestContext<object> requestContext = new RequestContext<object>(this, _minecraftContext);
            requestContext.Value = value;
            requestContext.ServiceProvider = _serviceScopeFactory.CreateScope().ServiceProvider;
            foreach (var endpoint in source.NotRouteEndpoints)
            {
                await endpoint.RequestDelegate.Invoke(requestContext);

                if (requestContext.NextRoute != null)
                {
                    requestContext.NextRoute = null;
                    await Run<T>(requestContext.NextRoute);
                    return;
                }

                if (requestContext.EndData != null)
                {
                    //Обработать окончание запроса
                    return;
                }
            }
        }

        public async Task Run<T>(string route) where T : ISourceHandler
        {
            var source = _piplineSources[typeof(T)];
            source.Memory();
            RequestContext<object> requestContext = new RequestContext<object>(this, _minecraftContext);
            requestContext.ServiceProvider = _serviceScopeFactory.CreateScope().ServiceProvider;
            var endpoint = source.RouteEndpoints.First(p => p.Route == route);

            await endpoint.RequestDelegate.Invoke(requestContext);

            if (requestContext.NextRoute != null)
            {
                requestContext.NextRoute = null;
                await Run<T>(requestContext.NextRoute);
                return;
            }

            if (requestContext.EndData != null)
            {
                //Обработать окончание запроса
                return;
            }
        }

        public async Task Run<T>(string route, object value) where T : ISourceHandler
        {
            var source = _piplineSources[typeof(T)];
            source.Memory();
            RequestContext<object> requestContext = new RequestContext<object>(this, _minecraftContext);
            requestContext.Value = value;
            requestContext.ServiceProvider = _serviceScopeFactory.CreateScope().ServiceProvider;
            var endpoint = source.RouteEndpoints.First(p => p.Route == route);

            await endpoint.RequestDelegate.Invoke(requestContext);

            if (requestContext.NextRoute != null)
            {
                await Run<T>(requestContext.NextRoute);
                requestContext.NextRoute = null;
                return;
            }

            if (requestContext.EndData != null)
            {
                //Обработать окончание запроса
                return;
            }
        }

        public async Task Start<T>(CancellationToken token) where T : ISourceHandler
        {
            var source = _piplineSources[typeof(T)];
            source.SourceHandler.OnSource += p => Run<T>(p).Wait();
            source.SourceHandler.Start(token);
        }

        public async Task Start<T>(string route, CancellationToken token) where T : ISourceHandler
        {
            var source = _piplineSources[typeof(T)];
            source.SourceHandler.OnSource += p => Run<T>(route, p).Wait();
            source.SourceHandler.Start(token);
        }

        public Task StopAll()
        {
            throw new NotImplementedException();
        }
    }
}
