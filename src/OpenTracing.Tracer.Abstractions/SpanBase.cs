using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTracing.Tracer.Abstractions
{
    /// <summary>
    /// A span base class that does NOT store logs and tags.
    /// See <see cref="SpanBaseWithDetails"/> for a base class that DOES store logs and tags.
    /// </summary>
    public abstract class SpanBase : ISpan
    {
        private const string LogKeyEvent = "event";

        private readonly TracerBase _tracer;
        private readonly SpanContextBase _typedContext;

        protected SpanDuration Duration { get; }

        public ISpanContext Context => _typedContext;

        public string OperationName { get; private set; }

        public DateTime StartTimestamp => Duration.StartTimestamp;
        public DateTime? FinishTimestamp { get; private set; }

        protected SpanBase(TracerBase tracer, SpanContextBase spanContext, string operationName, DateTime? startTimestamp)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (spanContext == null)
                throw new ArgumentNullException(nameof(spanContext));

            if (string.IsNullOrWhiteSpace(operationName))
                throw new ArgumentNullException(nameof(operationName));

            _tracer = tracer;
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
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException(nameof(eventName));

            return LogHelper(null, new Dictionary<string, object> { { LogKeyEvent, eventName } });
        }

        public ISpan Log(DateTime timestamp, string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
                throw new ArgumentNullException(nameof(eventName));

            return LogHelper(timestamp, new Dictionary<string, object> { { LogKeyEvent, eventName } });
        }

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            return LogHelper(null, fields);
        }

        public ISpan Log(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            return LogHelper(timestamp, fields);
        }

        private ISpan LogHelper(DateTime? timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            if (fields == null || !fields.Any())
                return this;

            DateTime validatedTimestamp = Duration.GetTimestamp(timestamp);

            LogInternal(validatedTimestamp, fields);
            return this;
        }

        /// <summary>
        /// This will only be called if the timestamp is valid and if <paramref name="fields"/> contains enries.
        /// </summary>
        protected abstract void LogInternal(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields);

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

        private void FinishInternal(DateTime? finishTimestamp)
        {
            if (FinishTimestamp.HasValue)
                return;

            FinishTimestamp = Duration.GetTimestamp(finishTimestamp);

            _tracer.SpanFinished(this);
        }
    }
}