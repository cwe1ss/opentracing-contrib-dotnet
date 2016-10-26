using System;
using System.Net;
using OpenTracing.Contrib.TracerAbstractions;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class ZipkinTracer : TracerBase
    {
        private readonly ZipkinTracerOptions _options;
        public ISpanReporter Reporter { get; }

        public Endpoint Endpoint { get; }

        public ZipkinTracer(ZipkinTracerOptions options, ISpanReporter reporter)
            : base(options)
        {
            if (reporter == null)
                throw new ArgumentNullException(nameof(reporter));

            _options = options;

            Reporter = reporter;

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


    }
}