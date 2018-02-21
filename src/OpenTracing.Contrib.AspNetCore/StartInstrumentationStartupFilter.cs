using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using OpenTracing.Contrib.Core;

namespace OpenTracing.Contrib.AspNetCore
{
    public class StartInstrumentationStartupFilter : IStartupFilter
    {
        private readonly IInstrumentor _instrumentor;

        public StartInstrumentationStartupFilter(IInstrumentor instrumentor)
        {
            _instrumentor = instrumentor ?? throw new ArgumentNullException(nameof(instrumentor));
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            _instrumentor.Start();

            return next;
        }
    }
}
