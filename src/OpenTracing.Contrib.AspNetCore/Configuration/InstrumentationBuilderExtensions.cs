using System;
using Microsoft.AspNetCore.Hosting;
using OpenTracing.Contrib;
using OpenTracing.Contrib.AspNetCore;
using OpenTracing.Contrib.AspNetCore.Configuration;
using OpenTracing.Contrib.AspNetCore.Mvc;
using OpenTracing.Contrib.EntityFrameworkCore;
using OpenTracing.Contrib.Http;

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

        /// <summary>
        /// Adds instrumentation for ASP.NET Core with default options.
        /// </summary>
        public static IInstrumentationBuilder AddAspNetCore(this IInstrumentationBuilder builder)
        {
            return AddAspNetCore(builder, new AspNetCoreOptions());
        }

        /// <summary>
        /// Adds instrumentation for ASP.NET Core with the given options.
        /// </summary>
        public static IInstrumentationBuilder AddAspNetCore(this IInstrumentationBuilder builder, Action<AspNetCoreOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var optionsInstance = new AspNetCoreOptions();
            options(optionsInstance);

            return AddAspNetCore(builder, optionsInstance);
        }

        /// <summary>
        /// Adds instrumentation for ASP.NET Core with the given options.
        /// </summary>
        public static IInstrumentationBuilder AddAspNetCore(this IInstrumentationBuilder builder, AspNetCoreOptions options)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            builder.Services.AddSingleton<IDiagnosticInterceptor, RequestInterceptor>();
            builder.Services.AddSingleton<IDiagnosticInterceptor, MvcInterceptor>();

            if (options.StartAutomatically)
            {
                builder.Services.AddTransient<IStartupFilter, StartInstrumentationStartupFilter>();
            }

            return builder;
        }
    }
}
