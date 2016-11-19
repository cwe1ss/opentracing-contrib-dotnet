using OpenTracing.Tracer.Abstractions;
using OpenTracing.Tracer.Zipkin.Propagation;
using OpenTracing.Propagation;
using System.Net;
using OpenTracing.Tracer.Zipkin.Json;
using System;
using OpenTracing.Tracer.Zipkin.Sampling;

namespace OpenTracing.Tracer.Zipkin.Configuration
{
    public class ZipkinTracerOptions : TracerOptions
    {
        /// <summary>
        /// This name will be used as the service name in the Zipkin UI.
        /// </summary>
        public string ServiceName { get; set; }

        public IPAddress ServiceIpAddress { get; set; }

        public ushort ServicePort { get; set; }

        public string ZipkinUri { get; set; }

        public IReporter Reporter { get; set; }

        public ISampler Sampler { get; set; }

        public ZipkinTracerOptions()
        {
            // Defaults
            Propagators.Add(Formats.TextMap.Name, new TextMapPropagator());
            Propagators.Add(Formats.HttpHeaders.Name, new TextMapPropagator());
        }

        public ZipkinTracerOptions WithServiceName(string serviceName)
        {
            ServiceName = serviceName;
            return this;
        }

        public ZipkinTracerOptions WithServiceEndpoint(IPAddress ipAddress, ushort port)
        {
            ServiceIpAddress = ipAddress;
            ServicePort = port;
            return this;
        }

        public ZipkinTracerOptions WithZipkinUri(string baseAddress)
        {
            ZipkinUri = baseAddress;
            return this;
        }

        public ZipkinTracerOptions WithJsonReporter(Action<JsonReporterOptions> options)
        {
            var jsonOptions = new JsonReporterOptions();
            options?.Invoke(jsonOptions);

            return WithJsonReporter(jsonOptions);
        }

        public ZipkinTracerOptions WithJsonReporter(JsonReporterOptions options = null)
        {
            Reporter = new JsonReporter(this, options ?? new JsonReporterOptions());
            return this;
        }

        public ZipkinTracerOptions WithConstSampler(bool samplingEnabled)
        {
            Sampler = new ConstSampler(samplingEnabled);
            return this;
        }

        public ZipkinTracerOptions WithProbabilisticSampler(double samplingRate)
        {
            Sampler = new ProbabilisticSampler(samplingRate);
            return this;
        }
    }
}