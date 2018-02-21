using System;
using OpenTracing.Contrib.Core;
using OpenTracing.Contrib.Core.Interceptors.EntityFrameworkCore;
using OpenTracing.Contrib.Core.Interceptors.HttpOut;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OpenTracingBuilderExtensions
    {
        /// <summary>
        /// Traces Entity Framework Core commands.
        /// </summary>
        public static IOpenTracingBuilder AddEntityFrameworkCore(this IOpenTracingBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IDiagnosticInterceptor, EntityFrameworkCoreInterceptor>();

            return builder;
        }

        /// <summary>
        /// Traces outgoing HTTP calls.
        /// </summary>
        public static IOpenTracingBuilder AddHttpClient(this IOpenTracingBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IDiagnosticInterceptor, HttpOutInterceptor>();

            return builder;
        }
    }
}
