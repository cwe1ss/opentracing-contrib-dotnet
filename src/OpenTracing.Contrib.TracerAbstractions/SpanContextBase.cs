using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenTracing.Contrib.TracerAbstractions
{
    public abstract class SpanContextBase : ISpanContext
    {
        /// <summary>
        /// The baggage for this context. Returns <c>null</c> if there are no entries!
        /// </summary>
        protected Dictionary<string, string> Baggage { get; private set; }

        /// <summary>
        /// We need the same HighResDateTime instance for all spans in one request,
        /// otherwise two spans could have overlapping/wrong start timestamps.
        /// </summary>
        public HighResClock Clock { get; }

        protected SpanContextBase(Dictionary<string, string> baggage, HighResClock clock)
        {
            Clock = clock ?? new HighResClock();

            Baggage = baggage;
        }

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            return Baggage ?? Enumerable.Empty<KeyValuePair<string, string>>();
        }

        public string GetBaggageItem(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (Baggage == null)
                return null;

            string value;
            Baggage.TryGetValue(key, out value);
            return value;
        }

        public void SetBaggageItem(string key, string value)
        {
            if (Baggage == null)
                Baggage = new Dictionary<string, string>();

            Baggage[key] = value;
        }
    }
}