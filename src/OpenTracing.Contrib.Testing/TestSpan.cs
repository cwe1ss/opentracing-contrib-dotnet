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
        public IList<LogData> Logs { get; }

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
            Logs = new List<LogData>();
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

        public ISpan SetTag(string key, int value)
        {
            Tags[key] = value;
            return this;
        }

        public ISpan SetTag(string key, string value)
        {
            Tags[key] = value;
            return this;
        }

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            return Log(DateTime.UtcNow, fields);
        }

        public ISpan Log(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            if (fields == null)
                return this;

            Logs.Add(new LogData(timestamp, fields));
            return this;
        }

        public ISpan Log(string eventName)
        {
            return Log(DateTime.UtcNow, eventName);
        }

        public ISpan Log(DateTime timestamp, string eventName)
        {
            return Log(timestamp, new Dictionary<string, object> { { "event", eventName }});
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

        public void Finish()
        {
            Finish(DateTime.UtcNow);
        }

        public void Finish(DateTime finishTimestamp)
        {
            FinishTimestamp = finishTimestamp;
            Tracer.FinishedSpans.Add(this);
        }

        public void Dispose()
        {
            Finish();
        }

        public class LogData
        {
            public DateTime Timestamp { get; }
            public IEnumerable<KeyValuePair<string, object>> Fields { get; }

            public LogData(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields)
            {
                Timestamp = timestamp;
                Fields = fields;
            }
        }
    }
}