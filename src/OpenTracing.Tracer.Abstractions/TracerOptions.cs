using System.Collections.Generic;

namespace OpenTracing.Tracer.Abstractions
{
    public abstract class TracerOptions
    {
        public Dictionary<string, IPropagator> Propagators { get; } = new Dictionary<string, IPropagator>();
    }
}