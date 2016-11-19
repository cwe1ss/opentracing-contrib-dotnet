using System;
using System.Net;
using OpenTracing.Tracer.Abstractions;
using OpenTracing.Tracer.Zipkin.Configuration;
using OpenTracing.Tracer.Zipkin.Json;
using OpenTracing.Tracer.Zipkin.Sampling;

namespace OpenTracing.Tracer.Zipkin
{
    public class ZipkinTracer : TracerBase
    {
        private readonly ZipkinTracerOptions _options;
        private readonly IReporter _reporter;

        public Endpoint Endpoint { get; }

        public ISampler Sampler { get; }

        public ZipkinTracer(ZipkinTracerOptions options, IEndpointResolver endpointResolver)
            : base(options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (endpointResolver == null)
                throw new ArgumentNullException(nameof(endpointResolver));

            _options = options;

            // TODO default reporter & sampler ?

            _reporter = options.Reporter ?? new JsonReporter(options, new JsonReporterOptions());

            Sampler = options.Sampler ?? new ConstSampler(true);

            Endpoint = CreateEndpoint(endpointResolver, options);
        }

        public override ISpanBuilder BuildSpan(string operationName)
        {
            return new ZipkinSpanBuilder(this, operationName);
        }

        public override void SpanFinished(SpanBase span)
        {
            var typedSpan = (ZipkinSpan)span;
            if (typedSpan.TypedContext.Sampled)
            {
                _reporter.Report(span);
            }
        }

        private Endpoint CreateEndpoint(IEndpointResolver endpointResolver, ZipkinTracerOptions options)
        {
            // get defaults
            var defaultEndpoint = endpointResolver.GetEndpoint();

            // overwrite with values from user-supplied options
            return new Endpoint
            {
                ServiceName = options.ServiceName ?? defaultEndpoint?.ServiceName ?? "Unknown",
                IPAddress = options.ServiceIpAddress ?? defaultEndpoint?.IPAddress ?? IPAddress.Loopback,
                Port = options.ServicePort != 0 ? options.ServicePort : defaultEndpoint?.Port ?? 0
            };
        }
    }
}