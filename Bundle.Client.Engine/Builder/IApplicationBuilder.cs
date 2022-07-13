using Microsoft.Extensions.Logging;

namespace Bundle.Client.Builder
{
    public interface IApplicationBuilder
    {
        public ILogger Logger { get; }
    }
}
