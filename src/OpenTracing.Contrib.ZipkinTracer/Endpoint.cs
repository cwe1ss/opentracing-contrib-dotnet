using System.Net;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class Endpoint
    {
        public IPAddress IPAddress { get; set; }

        public ushort Port { get; set; }

        public string ServiceName { get; set; }
    }
}