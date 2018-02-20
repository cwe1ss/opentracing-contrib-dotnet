using System;
using OpenTracing.Contrib;
using OpenTracing.Contrib.AspNetCore;
using OpenTracing.Contrib.AspNetCore.Mvc;
using OpenTracing.Contrib.EntityFrameworkCore;
using OpenTracing.Contrib.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InstrumentationBuilderExtensions
    {
        /// <summary>
        /// Adds instrumentation for ASP.NET Core (Incoming requests and MVC).
        /// </summary>
        public static IInstrumentationBuilder AddAspNetCore(this IInstrumentationBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IDiagnosticInterceptor, RequestInterceptor>();
            builder.Services.AddSingleton<IDiagnosticInterceptor, MvcInterceptor>();

            return builder;
        }

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
