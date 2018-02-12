using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTracing.Tracer
{
    /// <summary>
    /// A span base class that stores logs and tags.
    /// See <see cref="SpanBase"/> for a base class that does NOT store logs and tags.
    /// </summary>
    public abstract class SpanBaseWithDetails : SpanBase
    {
        private List<LogData> _logs;
        private KeyValueListNode<object> _tags;

        /// <summary>
        /// A (potentially empty) list of log entries. This will never return <c>null</c>.
        /// </summary>
        public IEnumerable<LogData> Logs
        {
            get { return _logs ?? Enumerable.Empty<LogData>(); }
        }

        /// <summary>
        /// A (potentially empty) list of tags. This will never return <c>null</c>.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Tags
        {
            get
            {
                for (var tags = _tags; tags != null; tags = tags.Next)
                    yield return tags.KeyValue;
            }
        }

        protected SpanBaseWithDetails(TracerBase tracer, SpanContextBase spanContext, string operationName, DateTimeOffset? startTimestamp,
            KeyValueListNode<object> tags)
            : base(tracer, spanContext, operationName, startTimestamp)
        {
            _tags = tags;
        }

        public override ISpan SetTag(string key, bool value)
        {
            _tags = new KeyValueListNode<object>() { KeyValue = new KeyValuePair<string, object>(key, value), Next = _tags };
            return this;
        }

        public override ISpan SetTag(string key, double value)
        {
            _tags = new KeyValueListNode<object>() { KeyValue = new KeyValuePair<string, object>(key, value), Next = _tags };
            return this;
        }

        public override ISpan SetTag(string key, int value)
        {
            _tags = new KeyValueListNode<object>() { KeyValue = new KeyValuePair<string, object>(key, value), Next = _tags };
            return this;
        }

        public override ISpan SetTag(string key, string value)
        {
            _tags = new KeyValueListNode<object>() { KeyValue = new KeyValuePair<string, object>(key, value), Next = _tags };
            return this;
        }

        protected override void LogInternal(DateTimeOffset timestamp, IDictionary<string, object> fields)
        {
            if (_logs == null)
                _logs = new List<LogData>();

            _logs.Add(new LogData(timestamp, fields));
        }
    }
}
