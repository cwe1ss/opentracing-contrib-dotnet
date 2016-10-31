using OpenTracing.Tracer.Abstractions;
using OpenTracing.Tracer.Zipkin.Propagation;
using OpenTracing.Propagation;

namespace OpenTracing.Tracer.Zipkin
{
    public class ZipkinTracerOptions : TracerOptions
    {
        /// <summary>
        /// This name will be used as the service name in the Zipkin UI.
        /// </summary>
        public string ServiceName { get; set; }

        public ZipkinTracerOptions()
        {
            ServiceName = "Unknown";

            Propagators.Add(Formats.TextMap.Name, new TextMapPropagator());
            Propagators.Add(Formats.HttpHeaders.Name, new TextMapPropagator());
        }
    }
}