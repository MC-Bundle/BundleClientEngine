using Bundle.Client.Routing;
using Bundle.Client.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Pipline
{
    public class PiplineSource
    {
        public string Route { get; set; }
        public ISourceHandler SourceHandler { get; set; }
        public List<RouteEndpoint> Endpoints { get; set; }

        public List<RouteEndpoint> NotRouteEndpoints { get; set; }
        public List<RouteEndpoint> RouteEndpoints { get; set; }

        public void Memory()
        {
            if(NotRouteEndpoints == null)
                NotRouteEndpoints = Endpoints.Where(e => e.Route == null).ToList();

            if(RouteEndpoints == null)
                RouteEndpoints = Endpoints.Where(e => e.Route != null).ToList();
        }
    }
}
