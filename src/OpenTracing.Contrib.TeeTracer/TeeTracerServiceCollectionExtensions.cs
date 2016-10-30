using System;
using OpenTracing;
using OpenTracing.Contrib.TeeTracer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TeeTracerServiceCollectionExtensions
    {
        public static IServiceCollection AddTeeTracer(IServiceCollection services, Action<TeeTracerOptions> options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var teeTracerOptions = new TeeTracerOptions();
            options?.Invoke(teeTracerOptions);

            return AddTeeTracer(services, teeTracerOptions);
        }

        public static IServiceCollection AddTeeTracer(IServiceCollection services, TeeTracerOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            services.AddSingleton(options);
            services.AddSingleton<ITracer, TeeTracer>();

            return services;
        }
    }
}