using System.Collections.Generic;

namespace OpenTracing.Contrib.TracerAbstractions
{
    public abstract class TracerOptions
    {
        public Dictionary<string, IPropagator> Propagators { get; } = new Dictionary<string, IPropagator>();
    }
}