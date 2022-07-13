using Bundle.Client.Pipline;
using Bundle.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Routing
{
    public class RequestContext : IRequestContext
    {
        private readonly IPipelineApplication _pipelineApplication;

        public RequestContext(IPipelineApplication pipelineApplication, MinecraftContext minecraftContext)
        {
            _pipelineApplication = pipelineApplication;
            MinecraftContext = minecraftContext;
        }

        public MinecraftContext MinecraftContext { get; }
        public RouteEndpoint Endpoint { get; set; }

        public IServiceProvider ServiceProvider { get; set; }
        public RequestClientData RequestData { get; set; }

        public EndData? EndData { get; set; }
        public void End(EndStatus endStatus, string message)
        {
            EndData = new EndData(endStatus, message);
        }

        public string NextRoute { get; set; }

        public void Next(string route)
        {
            NextRoute = route;
        }
    }

    public struct EndData
    {
        public EndStatus EndStatus;
        public string Message;

        public EndData(EndStatus endStatus, string message)
        {
            EndStatus = endStatus;
            Message = message;
        }
    }


    public class RequestContext<T> : RequestContext, IRequestContext<T>
    {
        private readonly IPipelineApplication _pipelineApplication;
        private readonly MinecraftContext _minecraftContext;
        public RequestContext(IPipelineApplication pipelineApplication, MinecraftContext minecraftContext) : base(pipelineApplication, minecraftContext)
        {
            _pipelineApplication = pipelineApplication;
            _minecraftContext = minecraftContext;
        }

        public T Value { get; set; }

        public IRequestContext<V> Convert<V>() where V : T
        {
            return new RequestContext<V>(_pipelineApplication, _minecraftContext)
            {
                EndData = EndData,
                NextRoute = NextRoute,
                Value = (V)Value,
                RequestData = RequestData
            };
        }
    }

    public class RequestClientData
    {
        public PacketTypeIn PacketType { get; set; }
    }

    public interface IRequestContext
    {
        public MinecraftContext MinecraftContext { get; }
        public RouteEndpoint Endpoint { get; internal set; }
        public RequestClientData RequestData { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        internal EndData? EndData { get; set; }
        public void End(EndStatus endStatus, string message = null);

        internal string NextRoute { get; set; }
        public void Next(string route);
    }

    public interface IRequestContext<T> : IRequestContext
    {
        public T Value { get; set; }
        public IRequestContext<V> Convert<V>() where V : T;
    }
}
