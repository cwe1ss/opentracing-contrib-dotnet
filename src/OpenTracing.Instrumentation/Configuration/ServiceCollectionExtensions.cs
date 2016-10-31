using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTracing.Instrumentation;
using OpenTracing.Instrumentation.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IOpenTracingBuilder AddOpenTracing(this IServiceCollection services, bool addDefaultInterceptors = true)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<ITraceContext, TraceContext>();

            var builder = new OpenTracingBuilder(services);

            if (addDefaultInterceptors)
            {
                builder.AddEntityFrameworkCore();
                builder.AddHttpClient();
            }

            return builder;
        }

        public static void StartOpenTracing(this IServiceProvider serviceProvider)
        {
            // TODO @cweiss this must be done different.

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var inspectors = serviceProvider.GetServices<IDiagnosticInterceptor>();
            foreach (var inspector in inspectors)
            {
                inspector.Start();
            }
        }
    }
}