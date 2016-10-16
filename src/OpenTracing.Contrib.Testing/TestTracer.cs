using System;
using System.Collections.Generic;
using OpenTracing.Propagation;

namespace OpenTracing.Contrib.Testing
{
    public class TestTracer : ITracer
    {
        public bool InjectCalled { get; private set; }

        public List<TestSpan> FinishedSpans { get; } = new List<TestSpan>();

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new TestSpanBuilder(this, operationName);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, Format<TCarrier> format, TCarrier carrier)
        {
            InjectCalled = true;
        }

        public ISpanContext Extract<TCarrier>(Format<TCarrier> format, TCarrier carrier)
        {
            throw new NotImplementedException();
        }
    }
}