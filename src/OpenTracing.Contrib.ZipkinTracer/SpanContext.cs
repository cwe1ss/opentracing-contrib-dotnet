using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTracing.Contrib.ZipkinTracer
{
    public class SpanContext : ISpanContext
    {
        private Dictionary<string, string> _baggage;

        public ulong TraceId { get; }
        public ulong SpanId { get; }
        public ulong? ParentId {get; }
        public bool Sampled { get; } // TODO @cweiss not yet used.

        // We need the same HighResDateTime instance for all spans in one request,
        // otherwise two spans could have overlapping/wrong start timestamps.
        public HighResClock Clock { get; }

        public SpanContext(ulong traceId, ulong spanId, ulong? parentId, Dictionary<string, string> baggage = null)
            : this(traceId, spanId, parentId, baggage, null)
        {
        }

        private SpanContext(ulong traceId, ulong spanId, ulong? parentId, Dictionary<string, string> baggage, HighResClock clock)
        {
            Clock = clock ?? new HighResClock();

            TraceId = traceId;
            SpanId = spanId;
            ParentId = parentId;

            _baggage = baggage;
        }

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            return _baggage ?? Enumerable.Empty<KeyValuePair<string, string>>();
        }

        public string GetBaggageItem(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (_baggage == null)
                return null;

            string value;
            _baggage.TryGetValue(key, out value);
            return value;
        }

        public void SetBaggageItem(string key, string value)
        {
            if (_baggage == null)
                _baggage = new Dictionary<string, string>();

            _baggage[key] = value;
        }

        public SpanContext CreateChild(ulong childSpanId)
        {
            return new SpanContext(TraceId, childSpanId, SpanId, _baggage, Clock);
        }
    }
}