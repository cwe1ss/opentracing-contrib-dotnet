using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace OpenTracing.Contrib.AspNetCore
{
    public class StartInstrumentationStartupFilter : IStartupFilter
    {
        private readonly IInstrumentor _instrumentor;

        public StartInstrumentationStartupFilter(IInstrumentor instrumentor)
        {
            if (instrumentor == null)
                throw new ArgumentNullException(nameof(instrumentor));

            _instrumentor = instrumentor;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            _instrumentor.Start();

            return next;
        }
    }
}
