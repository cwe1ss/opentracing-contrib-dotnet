using System.Collections.Generic;
using OpenTracing.Propagation;

namespace OpenTracing.Tracer
{
    public abstract class TracerOptions
    {
        public Dictionary<IFormat, IPropagator> Propagators { get; } = new Dictionary<IFormat, IPropagator>();
    }
}
