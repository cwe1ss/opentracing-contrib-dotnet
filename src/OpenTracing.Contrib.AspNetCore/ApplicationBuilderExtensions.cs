using System;
using OpenTracing.Contrib.AspNetCore;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseOpenTracing(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            app.UseMiddleware<OpenTracingMiddleware>();

            return app;
        }
    }
}