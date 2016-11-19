using System;
using System.Collections.Generic;
using System.Globalization;

namespace OpenTracing.Tracer.Zipkin.Sampling
{
    public class ProbabilisticSampler : ISampler
    {
        private double _samplingRate;
        private ulong _boundary;

        private Dictionary<string, string> _samplerTags;

        public ProbabilisticSampler(double samplingRate)
        {
            if (samplingRate < 0 || samplingRate > 1)
                throw new ArgumentOutOfRangeException(nameof(samplingRate), samplingRate, "Sampling Rate must be between 0 and 1");

            _samplingRate = samplingRate;

            _boundary = (ulong)(ulong.MaxValue * samplingRate);

            _samplerTags = new Dictionary<string, string> {
                { "sampler.type", "probabilistic" },
                { "sampler.param", samplingRate.ToString(CultureInfo.InvariantCulture) }
            };
        }

        public bool IsSampled(ulong traceId)
        {
            return traceId <= _boundary;
        }

        public IEnumerable<KeyValuePair<string, string>> GetTags()
        {
            return _samplerTags;
        }
    }
}