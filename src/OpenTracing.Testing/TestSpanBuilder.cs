using System;
using OpenTracing.Tracer;

namespace OpenTracing.Testing
{
    public class TestSpanBuilder : SpanBuilderBase
    {
        public TestTracer Tracer { get; }

        public TestSpanBuilder(TestTracer tracer, string operationName)
            : base(tracer, operationName)
        {
            Tracer = tracer;
        }

        protected override SpanBase CreateSpan()
        {
            TestSpanContext spanContext = new TestSpanContext(SpanReferences, null, null);

            var span = new TestSpan(Tracer, spanContext, OperationName, StartTimestamp, SpanTags);

            return span;
        }
    }
}
