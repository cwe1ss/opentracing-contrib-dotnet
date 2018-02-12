using System.Collections.Generic;
using OpenTracing.Tracer;

namespace OpenTracing.Testing
{
    public class TestSpanContext : SpanContextBase
    {
        public List<KeyValuePair<string, ISpanContext>> References { get; } = new List<KeyValuePair<string, ISpanContext>>();

        public TestSpanContext()
            : this(null, null, null)
        {
        }

        public TestSpanContext(KeyValueListNode<ISpanContext> references, Dictionary<string, string> baggage, IClock clock)
            : base(baggage, clock)
        {
            if (references != null)
            {
                foreach(var reference in references)
                {
                    References.Add(reference);
                }
            }
        }
    }
}
