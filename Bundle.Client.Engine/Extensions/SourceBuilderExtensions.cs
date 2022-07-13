using Bundle.Client.Builder;
using Bundle.Client.Builder;
using Bundle.Client.Pipline;
using Bundle.Client.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bundle.Client.Extensions
{
    public static class SourceBuilderExtensions
    {
        public static ISourceBuilder Use(this ISourceBuilder sourceBuilder, string route, RequestDelegate requestDelegate)
        {
            sourceBuilder.Add(new RouteEndpoint(requestDelegate, route));
            return sourceBuilder;
        }

        public static ISourceBuilder Use<T>(this ISourceBuilder sourceBuilder, string route, RequestDelegate<T> requestDelegate)
        {
            sourceBuilder.Use(route, p => {
                var conv = p.Convert<T>();
                requestDelegate.Invoke(conv);

                p.EndData = conv.EndData;
                p.NextRoute = conv.NextRoute;
                p.RequestData = conv.RequestData;
            });
            return sourceBuilder;
        }

        public static ISourceBuilder UseTrigger(this ISourceBuilder sourceBuilder, string route, TriggerRequestDelegate requestDelegate)
        {
            sourceBuilder.Use(route, p => requestDelegate.Invoke(p));
            return sourceBuilder;
        }

        public static ISourceBuilder Use(this ISourceBuilder sourceBuilder, string route, RequestVoidDelegate requestDelegate)
        {
            sourceBuilder.Add(new RouteEndpoint(async p => await Task.Run(() => requestDelegate.Invoke(p)), route));
            return sourceBuilder;
        }

        public static ISourceBuilder Use<T>(this ISourceBuilder sourceBuilder, string route, RequestVoidDelegate<T> requestDelegate)
        {
            sourceBuilder.Use(route, p => {
                var conv = p.Convert<T>();
                requestDelegate.Invoke(conv);

                p.EndData = conv.EndData;
                p.NextRoute = conv.NextRoute;
                p.RequestData = conv.RequestData;
            });
            return sourceBuilder;
        }

        public static ISourceBuilder UseTrigger(this ISourceBuilder sourceBuilder, string route, TriggerVoidRequestDelegate requestDelegate)
        {
            sourceBuilder.Use(route, p => requestDelegate.Invoke(p));
            return sourceBuilder;
        }

        public static ISourceBuilder Use(this ISourceBuilder sourceBuilder, RequestDelegate requestDelegate)
        {
            sourceBuilder.Add(new RouteEndpoint(requestDelegate, null));
            return sourceBuilder;
        }

        public static ISourceBuilder Use<T>(this ISourceBuilder sourceBuilder, RequestDelegate<T> requestDelegate)
        {
            sourceBuilder.Use(p => {
                var conv = p.Convert<T>();
                requestDelegate.Invoke(conv);

                p.EndData = conv.EndData;
                p.NextRoute = conv.NextRoute;
                p.RequestData = conv.RequestData;
            });
            return sourceBuilder;
        }

        public static ISourceBuilder UseTrigger(this ISourceBuilder sourceBuilder, TriggerRequestDelegate requestDelegate)
        {
            sourceBuilder.Use(p => requestDelegate.Invoke(p));
            return sourceBuilder;
        }

        public static ISourceBuilder Use(this ISourceBuilder sourceBuilder, RequestVoidDelegate requestDelegate)
        {
            sourceBuilder.Add(new RouteEndpoint(async p => await Task.Run(() => requestDelegate.Invoke(p)), null));
            return sourceBuilder;
        }

        public static ISourceBuilder Use<T>(this ISourceBuilder sourceBuilder, RequestVoidDelegate<T> requestDelegate)
        {
            sourceBuilder.Use(p =>
            {
                var conv = p.Convert<T>();
                requestDelegate.Invoke(conv);

                p.EndData = conv.EndData;
                p.NextRoute = conv.NextRoute;
                p.RequestData = conv.RequestData;
            });
            return sourceBuilder;
        }


        public static ISourceBuilder UseTrigger(this ISourceBuilder sourceBuilder, TriggerVoidRequestDelegate requestDelegate)
        {
            sourceBuilder.Use(p => requestDelegate.Invoke(p));
            return sourceBuilder;
        }


    }

}
