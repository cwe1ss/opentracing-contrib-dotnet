using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing.Tracer.Zipkin;
using OpenTracing.Util;
using zipkin4net;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport;
using zipkin4net.Transport.Http;

namespace Shared
{
    public static class ZipkinHelper
    {
        public static void ConfigureZipkin(IApplicationBuilder app)
        {
            // Zipkin Configuration

            var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();

            var zipkinLogger = new ZipkinLogger(loggerFactory, "zipkin4net");
            var zipkinSender = new HttpZipkinSender("http://localhost:9411", "application/json");
            var zipkinTracer = new ZipkinTracer(zipkinSender, new JSONSpanSerializer());

            TraceManager.RegisterTracer(zipkinTracer);
            TraceManager.Start(zipkinLogger);

            app.ApplicationServices.GetRequiredService<IApplicationLifetime>().ApplicationStopped.Register(() =>
            {
                TraceManager.Stop();
            });

            // OpenTracing -> Zipkin Configuration

            var otTracer = new OtTracer(
                new AsyncLocalScopeManager(),
                new ZipkinHttpTraceInjector(),
                new ZipkinHttpTraceExtractor());

            GlobalTracer.Register(otTracer);
        }
    }
}
