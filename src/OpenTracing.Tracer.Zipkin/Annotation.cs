using System;

namespace OpenTracing.Tracer.Zipkin
{
    /// <summary>
    /// An Annotation is used to record an occurance in time.
    /// </summary>
    public class Annotation
    {
        /// <summary>
        /// Timestamp marking the occurrence of an event.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Value holding an information about the annotation.
        /// See <see cref="AnnotationConstants"/> for some
        /// build in Zipkin annotation values.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Service endpoint.
        /// </summary>
        public Endpoint Endpoint { get; }

        public Annotation(DateTime timestamp, string value, Endpoint endpoint)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            Timestamp = timestamp;
            Value = value;
            Endpoint = endpoint;
        }
    }
}