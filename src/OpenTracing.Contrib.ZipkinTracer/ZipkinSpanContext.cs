using System.Collections.Generic;
using OpenTracing.Contrib.TracerAbstractions;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class ZipkinSpanContext : SpanContextBase
    {
        public ulong TraceId { get; }
        public ulong SpanId { get; }
        public ulong? ParentId {get; }
        public bool Sampled { get; } // TODO @cweiss not yet used.

        public ZipkinSpanContext(ulong traceId, ulong spanId, ulong? parentId, Dictionary<string, string> baggage = null)
            : this(traceId, spanId, parentId, baggage, null)
        {
        }

        private ZipkinSpanContext(ulong traceId, ulong spanId, ulong? parentId, Dictionary<string, string> baggage, HighResClock clock)
            : base(baggage, clock)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentId = parentId;
        }

        public ZipkinSpanContext CreateChild(ulong childSpanId)
        {
            return new ZipkinSpanContext(TraceId, childSpanId, SpanId, Baggage, Clock);
        }
    }
}