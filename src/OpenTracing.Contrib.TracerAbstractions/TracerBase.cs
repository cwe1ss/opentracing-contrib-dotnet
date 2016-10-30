using System;
using OpenTracing.Propagation;

namespace OpenTracing.Contrib.TracerAbstractions
{
    public abstract class TracerBase : ITracer
    {
        private readonly TracerOptions _options;

        protected TracerBase(TracerOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _options = options;
        }

        public abstract ISpanBuilder BuildSpan(string operationName);

        public abstract void ReportSpan(SpanBase span);

        public virtual void Inject<TCarrier>(ISpanContext spanContext, Format<TCarrier> format, TCarrier carrier)
        {
            if (spanContext == null)
                throw new ArgumentNullException(nameof(spanContext));

            if (carrier == null)
                throw new ArgumentNullException(nameof(carrier));

            IPropagator propagator;

            if (!_options.Propagators.TryGetValue(format.Name, out propagator))
            {
                throw new UnsupportedFormatException($"The format '{format.Name}' is not supported.");
            }

            propagator.Inject(spanContext, carrier);
        }

        public virtual ISpanContext Extract<TCarrier>(Format<TCarrier> format, TCarrier carrier)
        {
            if (carrier == null)
                throw new ArgumentNullException(nameof(carrier));

            IPropagator propagator;

            if (!_options.Propagators.TryGetValue(format.Name, out propagator))
            {
                throw new UnsupportedFormatException($"The format '{format.Name}' is not supported.");
            }

            return propagator.Extract(carrier);
        }
    }
}