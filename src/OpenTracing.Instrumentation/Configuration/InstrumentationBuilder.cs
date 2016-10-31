using System;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTracing.Instrumentation.Configuration
{
    public class InstrumentationBuilder : IInstrumentationBuilder
    {
        public IServiceCollection Services { get; }

        public InstrumentationBuilder(IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            Services = services;
        }
    }
}