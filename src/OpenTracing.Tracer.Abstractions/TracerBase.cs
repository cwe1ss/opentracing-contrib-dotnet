using System;
using System.Threading;
using OpenTracing.Propagation;

namespace OpenTracing.Tracer
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

        public IScopeManager ScopeManager { get; private set; }

        public ISpan ActiveSpan => ScopeManager?.Active?.Span;

        public abstract ISpanBuilder BuildSpan(string operationName);

        /// <summary>
        /// This method will be called whenever a span has been finished.
        /// </summary>
        public abstract void SpanFinished(SpanBase span);

        public virtual void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
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

        public virtual ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
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
