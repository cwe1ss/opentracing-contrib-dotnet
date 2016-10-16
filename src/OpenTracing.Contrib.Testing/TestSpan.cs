using System;
using System.Collections.Generic;

namespace OpenTracing.Contrib.Testing
{
    public class TestSpan : ISpan
    {
        public TestTracer Tracer { get; }
        public TestSpanContext TestContext { get; }
        public ISpanContext Context => TestContext;
        public string OperationName { get; private set; }
        public DateTime StartTimestamp { get; }
        public DateTime? FinishTimestamp { get; private set; }
        public IList<KeyValuePair<string, ISpanContext>> References { get; }
        public IDictionary<string, object> Tags { get; }

        public TestSpan(
            TestTracer tracer,
            TestSpanContext spanContext,
            DateTime startTimestamp,
            string operationName,
            IList<KeyValuePair<string, ISpanContext>> references,
            IDictionary<string, object> tags)
        {
            Tracer = tracer;
            TestContext = spanContext;
            StartTimestamp = startTimestamp;
            OperationName = operationName;
            References = references ?? new List<KeyValuePair<string, ISpanContext>>();
            Tags = tags ?? new Dictionary<string, object>();
        }

        public ISpan SetOperationName(string operationName)
        {
            OperationName = operationName;
            return this;
        }

        public ISpan SetTag(string key, bool value)
        {
            Tags[key] = value;
            return this;
        }

        public ISpan SetTag(string key, double value)
        {
            Tags[key] = value;
            return this;
        }

        public ISpan SetTag(string key, string value)
        {
            Tags[key] = value;
            return this;
        }

        public ISpan LogEvent(string eventName, object payload = null)
        {
            throw new NotImplementedException("will be removed in 0.9");
        }

        public ISpan LogEvent(DateTime timestamp, string eventName, object payload = null)
        {
            throw new NotImplementedException("will be removed in 0.9");
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            TestContext.SetBaggageItem(key, value);
            return this;
        }

        public string GetBaggageItem(string key)
        {
            return TestContext.GetBaggageItem(key);
        }

        public void Finish(DateTime? finishTimestamp = null)
        {
            FinishTimestamp = finishTimestamp ?? DateTime.UtcNow;
            Tracer.FinishedSpans.Add(this);
        }

        public void Dispose()
        {
            Finish();
        }
    }
}