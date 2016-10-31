using System;
using Newtonsoft.Json;

namespace OpenTracing.Tracer.Zipkin.Json
{
    internal class JsonAnnotation
    {
        private readonly Annotation _annotation;

        [JsonProperty("endpoint")]
        public JsonEndpoint Endpoint => new JsonEndpoint(_annotation.Endpoint);

        [JsonProperty("value")]
        public string Value => _annotation.Value;

        [JsonProperty("timestamp")]
        public long Timestamp => _annotation.Timestamp.ToUnixMicroseconds();

        public JsonAnnotation(Annotation annotation)
        {
            if (annotation == null)
                throw new ArgumentNullException(nameof(annotation));

            _annotation = annotation;
        }
    }
}