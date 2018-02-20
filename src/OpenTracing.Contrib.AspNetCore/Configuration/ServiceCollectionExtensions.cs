using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTracing;
using OpenTracing.Contrib;
using OpenTracing.Contrib.Configuration;
using OpenTracing.Util;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IInstrumentationBuilder AddOpenTracing(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var builder = services
                .AddOpenTracingCore()
                .AddEntityFrameworkCore()
                .AddHttpClient();

            return builder;
        }

        public static IInstrumentationBuilder AddOpenTracingCore(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<ITracer>(GlobalTracer.Instance);

            services.TryAddSingleton<IInstrumentor, Instrumentor>();

            var builder = new InstrumentationBuilder(services);

            return builder;
        }
    }
}
