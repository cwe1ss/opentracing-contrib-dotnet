using System;
using System.Linq;
using OpenTracing.Tracer.Abstractions;

namespace OpenTracing.Testing
{
    public class TestSpan : SpanBaseWithDetails
    {
        public TestSpanContext TypedContext => (TestSpanContext)Context;

        public TestSpan(
            TestTracer tracer,
            TestSpanContext spanContext,
            string operationName,
            DateTime? startTimestamp)
            : base(tracer, spanContext, operationName, startTimestamp)
        {
        }

        public bool? GetBoolTag(string key)
        {
            var kvp = BoolTags.FirstOrDefault(x => string.Equals(x.Key, key));
            return kvp.Key == null ? (bool?)null : kvp.Value;
        }

        public double? GetDoubleTag(string key)
        {
            var kvp = DoubleTags.FirstOrDefault(x => string.Equals(x.Key, key));
            return kvp.Key == null ? (double?)null : kvp.Value;
        }

        public int? GetIntTag(string key)
        {
            var kvp = IntTags.FirstOrDefault(x => string.Equals(x.Key, key));
            return kvp.Key == null ? (int?)null : kvp.Value;
        }

        public string GetStringTag(string key)
        {
            var kvp = StringTags.FirstOrDefault(x => string.Equals(x.Key, key));
            return kvp.Key == null ? null : kvp.Value;
        }
    }
}