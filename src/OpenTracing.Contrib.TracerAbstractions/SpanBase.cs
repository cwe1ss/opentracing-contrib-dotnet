using System;
using System.Collections.Generic;

namespace OpenTracing.Contrib.TracerAbstractions
{
    public abstract class SpanBase : ISpan
    {
        private readonly ISpanReporter _reporter;
        private readonly SpanContextBase _typedContext;

        protected SpanDuration Duration { get; }

        public ISpanContext Context => _typedContext;

        public string OperationName { get; private set; }

        public DateTime StartTimestamp => Duration.StartTimestamp;
        public DateTime? FinishTimestamp { get; private set; }

        protected SpanBase(ISpanReporter reporter, SpanContextBase spanContext, string operationName, DateTime? startTimestamp)
        {
            if (reporter == null)
                throw new ArgumentNullException(nameof(reporter));

            if (spanContext == null)
                throw new ArgumentNullException(nameof(spanContext));

            if (string.IsNullOrWhiteSpace(operationName))
                throw new ArgumentNullException(nameof(operationName));

            _reporter = reporter;
            _typedContext = spanContext;
            OperationName = operationName;
            Duration = new SpanDuration(spanContext.Clock, startTimestamp);
        }

        public ISpan SetOperationName(string operationName)
        {
            if (string.IsNullOrWhiteSpace(operationName))
                throw new ArgumentNullException(nameof(operationName));

            OperationName = operationName;
            return this;
        }

        public abstract ISpan SetTag(string key, string value);

        public abstract ISpan SetTag(string key, double value);

        public abstract ISpan SetTag(string key, int value);

        public abstract ISpan SetTag(string key, bool value);

        public ISpan Log(string eventName)
        {
            return Log(Duration.GetUtcNow(), eventName);
        }

        public ISpan Log(DateTime timestamp, string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException(nameof(eventName));

            Duration.ValidateTimestamp(timestamp);

            LogInternal(timestamp, new Dictionary<string, object> { { "event", eventName } });
            return this;
        }

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            return Log(Duration.GetUtcNow(), fields);
        }

        public ISpan Log(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            if (fields == null)
                return this;

            Duration.ValidateTimestamp(timestamp);

            LogInternal(timestamp, fields);
            return this;
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            _typedContext.SetBaggageItem(key, value);
            return this;
        }

        public string GetBaggageItem(string key)
        {
            return _typedContext.GetBaggageItem(key);
        }

        public void Finish()
        {
            FinishInternal(null);
        }

        public void Finish(DateTime finishTimestamp)
        {
            FinishInternal(finishTimestamp);
        }

        public void Dispose()
        {
            FinishInternal(null);
        }

        /// <summary>
        /// This will only be called if the timestamp is valid and if fields is not null.
        /// </summary>
        protected abstract void LogInternal(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields);

        private void FinishInternal(DateTime? finishTimestamp)
        {
            if (FinishTimestamp.HasValue)
                return;

            FinishTimestamp = Duration.Finish(finishTimestamp);

            _reporter.ReportSpan(this);
        }
    }
}