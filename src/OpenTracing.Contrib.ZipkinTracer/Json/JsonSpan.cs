using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace OpenTracing.Contrib.ZipkinTracer.Json
{
    internal class JsonSpan
    {
        private readonly Span _span;

        [JsonProperty("traceId")]
        public string TraceId => _span.TypedContext.TraceId.ToString("x4");

        [JsonProperty("name")]
        public string Name => _span.OperationName;

        [JsonProperty("id")]
        public string Id => _span.TypedContext.SpanId.ToString("x4");

        [JsonProperty("parentId", NullValueHandling = NullValueHandling.Ignore)]
        public string ParentId => _span.TypedContext.ParentId?.ToString("x4");

        [JsonProperty("timestamp")]
        public long Timestamp => _span.StartTimestamp.ToUnixMicroseconds();

        [JsonProperty("duration")]
        public long Duration => (_span.FinishTimestamp.Value - _span.StartTimestamp).Ticks / (TimeSpan.TicksPerMillisecond / 1000);

        [JsonProperty("annotations")]
        public IEnumerable<JsonAnnotation> Annotations =>
            _span.Annotations.Select(annotation => new JsonAnnotation(annotation));

        [JsonProperty("binaryAnnotations")]
        public IEnumerable<JsonBinaryAnnotation> BinaryAnnotations =>
            _span.BinaryAnnotations.Select(annotation => new JsonBinaryAnnotation(annotation));

        public JsonSpan(Span span)
        {
            if (span == null)
                throw new ArgumentNullException(nameof(span));

            this._span = span;
        }
    }
}