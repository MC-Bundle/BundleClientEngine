using Bundle.Client.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Routing
{
    public class RouteEndpoint
    {
        public RouteEndpoint(RequestDelegate requestDelegate, string route)
        {
            RequestDelegate = requestDelegate;
            Route = route;
        }

        public string Route { get; }
        public RequestDelegate RequestDelegate { get; }

        public DateTime CreatedAt = DateTime.Now;
    }
}
