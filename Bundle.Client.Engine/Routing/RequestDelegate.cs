using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Routing
{
    public delegate Task RequestDelegate<T>(IRequestContext<T> context);
    public delegate Task RequestDelegate(IRequestContext<object> context);
    public delegate Task TriggerRequestDelegate(IRequestContext context);

    public delegate void RequestVoidDelegate<T>(IRequestContext<T> context);
    public delegate void RequestVoidDelegate(IRequestContext<object> context);
    public delegate void TriggerVoidRequestDelegate(IRequestContext context);
}
