using System.Collections.Generic;

namespace OpenTracing.Tracer.Zipkin.Sampling
{
    public interface ISampler
    {
        bool IsSampled(ulong traceId);

        IEnumerable<KeyValuePair<string, string>> GetTags();
    }
}