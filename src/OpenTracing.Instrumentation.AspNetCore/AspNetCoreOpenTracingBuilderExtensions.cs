using System;
using OpenTracing.Instrumentation;
using OpenTracing.Instrumentation.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AspNetCoreOpenTracingBuilderExtensions
    {
        public static IOpenTracingBuilder AddAspNetCore(this IOpenTracingBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IDiagnosticInterceptor, RequestInterceptor>();

            return builder;
        }
    }
}