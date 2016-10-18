using System;
using System.Net;
using OpenTracing.Contrib.ZipkinTracer.Propagation;
using OpenTracing.Contrib.ZipkinTracer.Reporter;
using OpenTracing.Propagation;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class ZipkinTracer : ITracer
    {
        private readonly ZipkinTracerOptions _options;
        private readonly IReporter _reporter;

        public Endpoint Endpoint { get; }

        public ZipkinTracer(
            ZipkinTracerOptions options,
            IReporter reporter)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (reporter == null)
                throw new ArgumentNullException(nameof(reporter));

            Endpoint = new Endpoint
            {
                IPAddress = IPAddress.Loopback,
                Port = 5000,
                ServiceName = options.ServiceName
            };

            _options = options;
            _reporter = reporter;
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(this, operationName);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, Format<TCarrier> format, TCarrier carrier)
        {
            if (spanContext == null)
                throw new ArgumentNullException(nameof(spanContext));

            if (carrier == null)
                throw new ArgumentNullException(nameof(carrier));

            IPropagator propagator;

            if (!_options.Propagators.TryGetValue(format.Name, out propagator))
            {
                throw new UnsupportedFormatException($"The format '{format.Name}' is not supported.");
            }

            var typedContext = (SpanContext)spanContext;

            propagator.Inject(typedContext, carrier);
        }

        public ISpanContext Extract<TCarrier>(Format<TCarrier> format, TCarrier carrier)
        {
            if (carrier == null)
                throw new ArgumentNullException(nameof(carrier));

            IPropagator propagator;

            if (!_options.Propagators.TryGetValue(format.Name, out propagator))
            {
                throw new UnsupportedFormatException($"The format '{format.Name}' is not supported.");
            }

            return propagator.Extract(carrier);
        }

        public void ReportSpan(Span span)
        {
            if (span == null)
                throw new ArgumentNullException(nameof(span));

            _reporter.Report(span);
        }
    }
}