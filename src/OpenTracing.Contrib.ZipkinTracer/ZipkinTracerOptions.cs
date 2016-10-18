using System.Collections.Generic;
using OpenTracing.Contrib.ZipkinTracer.Propagation;
using OpenTracing.Propagation;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class ZipkinTracerOptions
    {
        /// <summary>
        /// This name will be used as the service name in the Zipkin UI.
        /// </summary>
        public string ServiceName { get; set; }

        public Dictionary<string, IPropagator> Propagators { get; } = new Dictionary<string, IPropagator>();

        public ZipkinTracerOptions()
        {
            Propagators.Add(Formats.TextMap.Name, new TextMapPropagator());
            Propagators.Add(Formats.HttpHeaders.Name, new TextMapPropagator());
        }
    }
}