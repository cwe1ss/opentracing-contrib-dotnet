using System;

namespace OpenTracing.Tracer.Zipkin
{
    /// <summary>
    /// Special annotation without time component. They can carry extra
    /// information i.e. when calling an HTTP service &rArr; URI of the call.
    /// </summary>
    public class BinaryAnnotation
    {
        /// <summary>
        /// Key of binary annotation.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Binary annotation's value.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Enum identifying type of value stored inside <see cref="Value"/> field.
        /// </summary>
        public AnnotationType AnnotationType { get; }

        /// <summary>
        /// Service endpoint.
        /// </summary>
        public Endpoint Endpoint { get; }

        public BinaryAnnotation(string key, object value, Endpoint endpoint)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Key = key;
            Value = value;
            AnnotationType = value.GetType().AsAnnotationType();
            Endpoint = endpoint;
        }
    }
}
