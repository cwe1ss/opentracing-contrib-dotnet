using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTracing;
using OpenTracing.Contrib;
using OpenTracing.Contrib.AspNetCore;
using OpenTracing.NullTracer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenTracing(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // OpenTracing.Contrib
            services.TryAddSingleton<ISpanAccessor, SpanAccessor>();

            // OpenTracing.Contrib.AspNetCore
            services.TryAddSingleton<IIncomingHttpOperationName, DefaultIncomingHttpOperationName>();

            return services;
        }

        public static IServiceCollection AddNullTracer(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Null Tracer if no other tracer is present.
            services.TryAddSingleton<ITracer>(_ => NullTracer.Instance);

            return services;
        }
    }
}