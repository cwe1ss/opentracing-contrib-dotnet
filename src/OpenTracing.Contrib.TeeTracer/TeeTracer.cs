using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTracing.Propagation;

namespace OpenTracing.Contrib.TeeTracer
{
    public class TeeTracer : ITracer
    {
        private readonly ITracer _mainTracer;
        private readonly ITracer _secondaryTracer;

        public TeeTracer(TeeTracerOptions options, IServiceProvider serviceProvider)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.MainTracer == null)
                throw new ArgumentNullException($"{nameof(options)}.{nameof(options.MainTracer)}");

            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            _mainTracer = ResolveTracer(options.MainTracer, serviceProvider);
            _secondaryTracer = ResolveTracer(options.SecondaryTracer, serviceProvider);
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            var mainSpanBuilder = _mainTracer.BuildSpan(operationName);
            var secondarySpanBuilder = _secondaryTracer?.BuildSpan(operationName);

            return new TeeSpanBuilder(mainSpanBuilder, secondarySpanBuilder);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, Format<TCarrier> format, TCarrier carrier)
        {
            // The secondary tracer MUST NOT update the carrier!
            // It's only called to allow it to update metrics etc. (e.g. Prometheus)

            _mainTracer.Inject<TCarrier>(spanContext, format, carrier);
            _secondaryTracer?.Inject<TCarrier>(spanContext, format, carrier);
        }

        public ISpanContext Extract<TCarrier>(Format<TCarrier> format, TCarrier carrier)
        {
            // The secondary tracer only gets the call to update metrics etc. (e.g. Prometheus)
            // Its result will not be used!

            var context = _mainTracer.Extract<TCarrier>(format, carrier);
            _secondaryTracer?.Extract<TCarrier>(format, carrier);
            return context;
        }

        private static ITracer ResolveTracer(Type tracerType, IServiceProvider serviceProvider)
        {
            if (tracerType == null)
                return null;

            return (ITracer)serviceProvider.GetRequiredService(tracerType);
        }
    }
}