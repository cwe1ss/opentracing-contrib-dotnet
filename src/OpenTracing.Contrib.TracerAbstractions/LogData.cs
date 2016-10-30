using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTracing.Contrib.TracerAbstractions
{
    /// <summary>
    /// Represents a single timestamped log entry in a <see cref="SpanBaseWithDetails"/>.
    /// </summary>
    public class LogData
    {
        public DateTime Timestamp { get; }
        public IEnumerable<KeyValuePair<string, object>> Fields { get; }

        public LogData(DateTime timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            Timestamp = timestamp;
            Fields = fields ?? Enumerable.Empty<KeyValuePair<string, object>>();
        }
    }
}