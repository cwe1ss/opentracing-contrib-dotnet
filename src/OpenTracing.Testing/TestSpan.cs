using System;
using System.Linq;
using OpenTracing.Tracer;

namespace OpenTracing.Testing
{
    public class TestSpan : SpanBaseWithDetails
    {
        public TestSpanContext TypedContext => (TestSpanContext)Context;

        public TestSpan(
            TestTracer tracer,
            TestSpanContext spanContext,
            string operationName,
            DateTimeOffset? startTimestamp,
            KeyValueListNode<object> tags)
            : base(tracer, spanContext, operationName, startTimestamp, tags)
        {
        }

        public bool? GetBoolTag(string key)
        {
            var kvp = base.Tags.FirstOrDefault(x => string.Equals(x.Key, key));
            return kvp.Key == null ? null : kvp.Value as bool?;
        }

        public double? GetDoubleTag(string key)
        {
            var kvp = base.Tags.FirstOrDefault(x => string.Equals(x.Key, key));
            return kvp.Key == null ? null : kvp.Value as double?;
        }

        public int? GetIntTag(string key)
        {
            var kvp = base.Tags.FirstOrDefault(x => string.Equals(x.Key, key));
            return kvp.Key == null ? null : kvp.Value as int?;
        }

        public string GetStringTag(string key)
        {
            var kvp = base.Tags.FirstOrDefault(x => string.Equals(x.Key, key));
            return kvp.Key == null ? null : kvp.Value as string;
        }
    }
}
