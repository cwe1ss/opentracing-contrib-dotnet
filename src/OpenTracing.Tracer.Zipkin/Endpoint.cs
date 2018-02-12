using System.Net;

namespace OpenTracing.Tracer.Zipkin
{
    public class Endpoint
    {
        public IPAddress IPAddress { get; set; }

        public ushort Port { get; set; }

        public string ServiceName { get; set; }
    }
}
