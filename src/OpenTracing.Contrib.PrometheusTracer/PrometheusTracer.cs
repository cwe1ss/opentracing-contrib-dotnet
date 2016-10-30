using System;
using OpenTracing.Propagation;

namespace OpenTracing.Contrib.PrometheusTracer
{
    public class PrometheusTracer : ITracer
    {
        public ISpanBuilder BuildSpan(string operationName)
        {
            throw new NotImplementedException();
        }

        public void Inject<TCarrier>(ISpanContext spanContext, Format<TCarrier> format, TCarrier carrier)
        {
            throw new NotImplementedException();
        }

        public ISpanContext Extract<TCarrier>(Format<TCarrier> format, TCarrier carrier)
        {
            throw new NotImplementedException();
        }
    }
}