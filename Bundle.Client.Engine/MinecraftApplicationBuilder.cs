using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Bundle.Client
{
    public class MinecraftApplicationBuilder
    {
        internal MinecraftApplicationBuilder(IConfiguration _configuration)
        {
            Configuration = _configuration;
            Services = new ServiceCollection();
        }


        public IConfiguration Configuration { get; }
        public IServiceCollection Services { get; }
        public MinecraftApplication Build()
        {

            var provider = Services.BuildServiceProvider();
            return new MinecraftApplication(provider);
        }


    }
}
