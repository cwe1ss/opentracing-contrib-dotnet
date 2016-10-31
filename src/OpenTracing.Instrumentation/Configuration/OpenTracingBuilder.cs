using System;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTracing.Instrumentation.Configuration
{
    public class OpenTracingBuilder : IOpenTracingBuilder
    {
        public IServiceCollection Services { get; }

        public OpenTracingBuilder(IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            Services = services;
        }
    }
}