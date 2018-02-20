using System;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTracing.Contrib.Configuration
{
    public class InstrumentationBuilder : IInstrumentationBuilder
    {
        public IServiceCollection Services { get; }

        public InstrumentationBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}
