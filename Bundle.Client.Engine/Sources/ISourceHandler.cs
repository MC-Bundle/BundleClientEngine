using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bundle.Client.Sources
{
    public interface ISourceHandler
    {
        public void Start(CancellationToken token);
        public void Stop();
        public delegate void SourceDelegate(object value);
        public event SourceDelegate OnSource;

    }
}
