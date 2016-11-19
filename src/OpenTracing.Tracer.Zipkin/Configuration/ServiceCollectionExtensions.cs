using System;
using OpenTracing;
using OpenTracing.Tracer.Zipkin;
using OpenTracing.Tracer.Zipkin.Configuration;

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
            services.AddSingleton<IEndpointResolver, DefaultEndpointResolver>();

            return services;
        }
    }
}