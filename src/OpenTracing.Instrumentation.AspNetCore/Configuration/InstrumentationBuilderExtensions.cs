using System;
using Microsoft.AspNetCore.Hosting;
using OpenTracing.Instrumentation;
using OpenTracing.Instrumentation.AspNetCore;
using OpenTracing.Instrumentation.AspNetCore.Configuration;
using OpenTracing.Instrumentation.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InstrumentationBuilderExtensions
    {
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