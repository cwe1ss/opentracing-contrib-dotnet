using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenTracing.Propagation;

namespace OpenTracing.Tracer
{
    public abstract class TracerOptions
    {
        private Dictionary<object, IPropagator> _propagators = new Dictionary<object, IPropagator>();

        public IReadOnlyDictionary<object, IPropagator> Propagators => new ReadOnlyDictionary<object, IPropagator>(_propagators);

        public void SetPropagator<TCarrier>(IFormat<TCarrier> format, IPropagator propagator)
        {
            _propagators[format] = propagator;
        }
    }
}
