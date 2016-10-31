using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTracing.Instrumentation;
using OpenTracing.Instrumentation.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IInstrumentationBuilder AddInstrumentation(this IServiceCollection services, bool addDefaultInterceptors = true)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Core services
            services.TryAddSingleton<ITraceContext, TraceContext>();
            services.TryAddSingleton<IInstrumentor, Instrumentor>();

            var builder = new InstrumentationBuilder(services);

            if (addDefaultInterceptors)
            {
                builder.AddEntityFrameworkCore();
                builder.AddHttpClient();
            }

            return builder;
        }
    }
}