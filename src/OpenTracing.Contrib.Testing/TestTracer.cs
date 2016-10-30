using System.Collections.Generic;
using OpenTracing.Contrib.TracerAbstractions;
using OpenTracing.Propagation;

namespace OpenTracing.Contrib.Testing
{
    public class TestTracer : TracerBase
    {
        public bool InjectCalled { get; private set; }

        public bool ExtractCalled { get; private set; }

        public List<TestSpan> FinishedSpans { get; } = new List<TestSpan>();

        public TestTracer()
            : base(new TestTracerOptions())
        {
        }

        public override ISpanBuilder BuildSpan(string operationName)
        {
            return new TestSpanBuilder(this, operationName);
        }

        public override void Inject<TCarrier>(ISpanContext spanContext, Format<TCarrier> format, TCarrier carrier)
        {
            InjectCalled = true;
        }

        public override ISpanContext Extract<TCarrier>(Format<TCarrier> format, TCarrier carrier)
        {
            ExtractCalled = true;
            return null;
        }

        public override void SpanFinished(SpanBase span)
        {
            FinishedSpans.Add((TestSpan)span);
        }
    }
}