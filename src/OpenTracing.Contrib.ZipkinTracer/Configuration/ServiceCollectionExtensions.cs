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

            // Allow the tracer implementation itself to be resolved.
            // This is important for the TeeTracer which needs the actual implementation.
            services.AddSingleton<ZipkinTracer>();

            // Use the same instance if someone resolves it through the ITracer interface.
            services.AddSingleton<ITracer>(provider => provider.GetRequiredService<ZipkinTracer>());

            services.TryAddSingleton<ISpanReporter, AwfulPoCReporter>();

            return services;
        }
    }
}