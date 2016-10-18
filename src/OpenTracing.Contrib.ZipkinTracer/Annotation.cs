using System;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class Annotation
    {
        public Endpoint Endpoint { get; }

        public DateTime Timestamp { get; }

        public string Value { get; }

        public Annotation(Endpoint endpoint, string value, DateTime timestamp)
        {
            Endpoint = endpoint;
            Value = value;
            Timestamp = timestamp;
        }
    }
}