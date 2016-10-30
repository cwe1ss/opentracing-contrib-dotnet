using System.Collections.Generic;
using OpenTracing.Contrib.TracerAbstractions;

namespace OpenTracing.Contrib.Testing
{
    public class TestSpanContext : SpanContextBase
    {
        public List<SpanReference> References { get; } = new List<SpanReference>();

        public TestSpanContext()
            : this(null, null, null)
        {
        }

        public TestSpanContext(IEnumerable<SpanReference> references, Dictionary<string, string> baggage, IClock clock)
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