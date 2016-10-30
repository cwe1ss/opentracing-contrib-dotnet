using System.Collections.Generic;
using System.Linq;

namespace OpenTracing.Contrib.PrometheusTracer
{
    public class PrometheusSpanContext : ISpanContext
    {
        public static readonly ISpanContext Instance = new PrometheusSpanContext();

        private readonly IEnumerable<KeyValuePair<string, string>> _baggage;

        private PrometheusSpanContext()
        {
            _baggage = Enumerable.Empty<KeyValuePair<string, string>>();
        }

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            return _baggage;
        }
    }
}