using System;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class Annotation
    {
        public Endpoint Endpoint { get; }

        public DateTime Timestamp { get; }

        public string Value { get; }

        public Annotation(Endpoint endpoint, DateTime timestamp, string value)
        {
            Endpoint = endpoint;
            Timestamp = timestamp;
            Value = value;
        }
    }
}