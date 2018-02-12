using System.Collections.Generic;
using OpenTracing.Tracer;

namespace OpenTracing.Tracer.Zipkin
{
    public class ZipkinSpanContext : SpanContextBase
    {
        /// <summary>
        /// The overall ID of the trace. Every span in a trace will share this ID.
        /// </summary>
        public ulong TraceId { get; }

        /// <summary>
        /// The ID for a particular span. This may or may not be the same as the trace id.
        /// </summary>
        public ulong SpanId { get; }

        /// <summary>
        /// This is an optional ID that will only be present on child spans.
        /// That is the span without a parent id is considered the root of the trace.
        /// </summary>
        public ulong? ParentId {get; }

        public bool Sampled { get; } // TODO @cweiss not yet used.

        public ZipkinSpanContext(ulong traceId, ulong spanId, ulong? parentId, bool sampled, Dictionary<string, string> baggage = null)
            : this(traceId, spanId, parentId, sampled, baggage, null)
        {
        }

        private ZipkinSpanContext(ulong traceId, ulong spanId, ulong? parentId, bool sampled, Dictionary<string, string> baggage, IClock clock)
            : base(baggage, clock)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentId = parentId;
            Sampled = sampled;
        }

        /// <summary>
        /// Creates a new context which as marked as a child of the current span.
        /// </summary>
        public ZipkinSpanContext CreateChild(ulong childSpanId)
        {
            return new ZipkinSpanContext(TraceId, childSpanId, SpanId, Sampled, Baggage, Clock);
        }
    }
}