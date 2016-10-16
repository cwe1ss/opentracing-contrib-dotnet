using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTracing;
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

            // Null Tracer if no other tracer is present.
            services.TryAddSingleton<ITracer, NullTracer>();

            services.TryAddSingleton<IIncomingHttpOperationName, DefaultIncomingHttpOperationName>();

            return services;
        }
    }
}