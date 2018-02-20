using System;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTracing.Contrib.AspNetCore.Configuration
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
