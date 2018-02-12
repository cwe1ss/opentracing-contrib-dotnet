using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTracing.Tracer
{
    /// <summary>
    /// A span base class that does NOT store logs and tags.
    /// See <see cref="SpanBaseWithDetails"/> for a base class that DOES store logs and tags.
    /// </summary>
    public abstract class SpanBase : ISpan
    {
        private const string LogKeyEvent = "event";

        private readonly TracerBase _tracer;
        private readonly SpanContextBase _context;

        protected SpanDuration Duration { get; }

        public ISpanContext Context => _context;

        internal ISpan Parent { get; set; }

        public string OperationName { get; private set; }

        public DateTimeOffset StartTimestamp => Duration.StartTimestamp;
        public DateTimeOffset? FinishTimestamp { get; private set; }

        protected SpanBase(TracerBase tracer, SpanContextBase spanContext, string operationName, DateTimeOffset? startTimestamp)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (spanContext == null)
                throw new ArgumentNullException(nameof(spanContext));

            if (string.IsNullOrWhiteSpace(operationName))
                throw new ArgumentNullException(nameof(operationName));

            _tracer = tracer;
            _context = spanContext;
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

        public ISpan Log(IDictionary<string, object> fields)
        {
            return LogHelper(null, fields);
        }

        public ISpan Log(DateTimeOffset timestamp, IDictionary<string, object> fields)
        {
            return LogHelper(timestamp, fields);
        }

        public ISpan Log(string @event)
        {
            if (string.IsNullOrWhiteSpace(@event))
                throw new ArgumentNullException(nameof(@event));

            return LogHelper(null, new Dictionary<string, object> { { LogKeyEvent, @event } });
        }

        public ISpan Log(DateTimeOffset timestamp, string @event)
        {
            if (string.IsNullOrWhiteSpace(@event))
                throw new ArgumentNullException(nameof(@event));

            return LogHelper(timestamp, new Dictionary<string, object> { { LogKeyEvent, @event } });
        }

        private ISpan LogHelper(DateTimeOffset? timestamp, IDictionary<string, object> fields)
        {
            if (fields == null || !fields.Any())
                return this;

            DateTimeOffset validatedTimestamp = Duration.GetTimestamp(timestamp);

            LogInternal(validatedTimestamp, fields);
            return this;
        }

        /// <summary>
        /// This will only be called if the timestamp is valid and if <paramref name="fields"/> contains entries.
        /// </summary>
        protected abstract void LogInternal(DateTimeOffset timestamp, IDictionary<string, object> fields);

        public ISpan SetBaggageItem(string key, string value)
        {
            _context.SetBaggageItem(key, value);
            return this;
        }

        public string GetBaggageItem(string key)
        {
            return _context.GetBaggageItem(key);
        }

        public void Finish()
        {
            FinishInternal(null);
        }

        public void Finish(DateTimeOffset finishTimestamp)
        {
            FinishInternal(finishTimestamp);
        }

        private void FinishInternal(DateTimeOffset? finishTimestamp)
        {
            if (FinishTimestamp.HasValue)
                return;

            FinishTimestamp = Duration.GetTimestamp(finishTimestamp);

            _tracer.SpanFinished(this);
        }
    }
}
