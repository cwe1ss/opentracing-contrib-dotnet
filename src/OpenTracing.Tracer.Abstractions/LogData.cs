using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTracing.Tracer
{
    /// <summary>
    /// Represents a single timestamped log entry in a <see cref="SpanBaseWithDetails"/>.
    /// </summary>
    public class LogData
    {
        public DateTimeOffset Timestamp { get; }
        public IEnumerable<KeyValuePair<string, object>> Fields { get; }

        public LogData(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            Timestamp = timestamp;
            Fields = fields ?? Enumerable.Empty<KeyValuePair<string, object>>();
        }
    }
}
