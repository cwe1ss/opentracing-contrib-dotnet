using System;
using OpenTracing.Instrumentation;
using OpenTracing.Instrumentation.EntityFrameworkCore;
using OpenTracing.Instrumentation.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InstrumentationBuilderExtensions
    {
        /// <summary>
        /// Traces Entity Framework Core commands.
        /// </summary>
        public static IInstrumentationBuilder AddEntityFrameworkCore(this IInstrumentationBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IDiagnosticInterceptor, EntityFrameworkCoreInterceptor>();

            return builder;
        }

        /// <summary>
        /// Traces outgoing HTTP calls.
        /// </summary>
        public static IInstrumentationBuilder AddHttpClient(this IInstrumentationBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IDiagnosticInterceptor, HttpHandlerInterceptor>();

            return builder;
        }
    }
}