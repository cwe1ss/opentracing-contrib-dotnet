using System.Collections.Generic;

namespace OpenTracing.Tracer.Zipkin.Sampling
{
    public class ConstSampler : ISampler
    {
        private bool _samplingEnabled;

        private Dictionary<string, string> _samplerTags;

        public ConstSampler(bool samplingEnabled)
        {
            _samplingEnabled = samplingEnabled;

            _samplerTags = new Dictionary<string, string> {
                { "sampler.type", "const" }
            };
        }

        public bool IsSampled(ulong traceId)
        {
            return _samplingEnabled;
        }

        public IEnumerable<KeyValuePair<string, string>> GetTags()
        {
            return _samplerTags;
        }
    }
}
