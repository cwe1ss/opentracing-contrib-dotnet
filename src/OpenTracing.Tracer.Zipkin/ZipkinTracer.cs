using System;
using System.Net;
using OpenTracing.Tracer.Abstractions;

namespace OpenTracing.Tracer.Zipkin
{
    public class ZipkinTracer : TracerBase
    {
        private readonly ZipkinTracerOptions _options;
        private readonly IReporter _reporter;

        public Endpoint Endpoint { get; }

        public ZipkinTracer(ZipkinTracerOptions options, IReporter reporter)
            : base(options)
        {
            if (reporter == null)
                throw new ArgumentNullException(nameof(reporter));

            _options = options;
            _reporter = reporter;

            // TODO @cweiss !!!
            Endpoint = new Endpoint
            {
                IPAddress = IPAddress.Loopback,
                Port = 5000,
                ServiceName = options.ServiceName
            };
        }

        public override ISpanBuilder BuildSpan(string operationName)
        {
            return new ZipkinSpanBuilder(this, operationName);
        }

        public override void SpanFinished(SpanBase span)
        {
            _reporter.Report(span);
        }
    }
}