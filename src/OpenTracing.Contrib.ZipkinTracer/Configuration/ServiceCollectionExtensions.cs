using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTracing;
using OpenTracing.Contrib.ZipkinTracer;
using OpenTracing.Contrib.ZipkinTracer.Reporter;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddZipkinTracer(this IServiceCollection services, Action<ZipkinTracerOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var zipkinOptions = new ZipkinTracerOptions();
            options(zipkinOptions);

            return AddZipkinTracer(services, zipkinOptions);
        }

        public static IServiceCollection AddZipkinTracer(this IServiceCollection services, ZipkinTracerOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            services.AddSingleton(options);
            services.AddSingleton<ITracer, ZipkinTracer>();

            services.TryAddSingleton<IReporter, AwfulPoCReporter>();
            
            return services;
        }
    }
}