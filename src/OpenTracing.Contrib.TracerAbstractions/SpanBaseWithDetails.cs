using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTracing.Contrib.TracerAbstractions
{
    /// <summary>
    /// A span base class that stores logs and tags.
    /// See <see cref="SpanBase"/> for a base class that does NOT store logs and tags.
    /// </summary>
    public abstract class SpanBaseWithDetails : SpanBase
    {
        private List<LogData> _logs;

        // Separate dictionaries to prevent boxing.
        private Dictionary<string, bool> _boolTags;
        private Dictionary<string, double> _doubleTags;
        private Dictionary<string, int> _intTags;
        private Dictionary<string, string> _stringTags;

        /// <summary>
        /// A (potentially empty) list of log entries. This will never return <c>null</c>.
        /// </summary>
        public IEnumerable<LogData> Logs
        {
            get { return _logs ?? Enumerable.Empty<LogData>(); }
        }

        /// <summary>
        /// A (potentially empty) list of <c>bool</c> tags. This will never return <c>null</c>.
        /// </summary>
        public IEnumerable<KeyValuePair<string, bool>> BoolTags
        {
            get { return _boolTags ?? Enumerable.Empty<KeyValuePair<string, bool>>(); }
        }

        /// <summary>
        /// A (potentially empty) list of <c>double</c> tags. This will never return <c>null</c>.
        /// </summary>
        public IEnumerable<KeyValuePair<string, double>> DoubleTags
        {
            get { return _doubleTags ?? Enumerable.Empty<KeyValuePair<string, double>>(); }
        }

        /// <summary>
        /// A (potentially empty) list of <c>int</c> tags. This will never return <c>null</c>.
        /// </summary>
        public IEnumerable<KeyValuePair<string, int>> IntTags
        {
            get { return _intTags ?? Enumerable.Empty<KeyValuePair<string, int>>(); }
        }

        /// <summary>
        /// A (potentially empty) list of <c>string</c> tags. This will never return <c>null</c>.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> StringTags
        {
            get { return _stringTags ?? Enumerable.Empty<KeyValuePair<string, string>>(); }
        }

        protected SpanBaseWithDetails(TracerBase tracer, SpanContextBase spanContext, string operationName, DateTime? startTimestamp)
            : base(tracer, spanContext, operationName, startTimestamp)
        {
        }

        public override ISpan SetTag(string key, bool value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_boolTags == null)
                _boolTags = new Dictionary<string, bool>();

            _boolTags[key] = value;
            return this;
        }

        public override ISpan SetTag(string key, double value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_doubleTags == null)
                _doubleTags = new Dictionary<string, double>();

            _doubleTags[key] = value;
            return this;
        }

        public override ISpan SetTag(string key, int value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_intTags == null)
                _intTags = new Dictionary<string, int>();

            _intTags[key] = value;
            return this;
        }

        public override ISpan SetTag(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (_stringTags == null)
                _stringTags = new Dictionary<string, string>();

            _stringTags[key] = value;
            return this;
        }

        protected override void LogInternal(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            if (_logs == null)
                _logs = new List<LogData>();

            _logs.Add(new LogData(timestamp, fields));
        }
    }
}