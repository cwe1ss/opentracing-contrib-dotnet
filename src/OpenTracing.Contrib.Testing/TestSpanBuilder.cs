using System;
using OpenTracing.Contrib.TracerAbstractions;

namespace OpenTracing.Contrib.Testing
{
    public class TestSpanBuilder : SpanBuilderBase
    {
        public TestTracer Tracer { get; }

        public TestSpanBuilder(TestTracer tracer, string operationName)
            : base(operationName)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            Tracer = tracer;
        }

        public override ISpan Start()
        {
            TestSpanContext spanContext = new TestSpanContext(SpanReferences, null, null);

            var span = new TestSpan(Tracer, spanContext, OperationName, StartTimestamp);

            SetSpanTags(span);

            return span;
        }
    }
}