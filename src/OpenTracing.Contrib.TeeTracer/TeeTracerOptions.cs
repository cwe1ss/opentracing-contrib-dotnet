using System;
using System.Reflection;

namespace OpenTracing.Contrib.TeeTracer
{
    public class TeeTracerOptions
    {
        public Type MainTracer { get; private set; }

        public Type SecondaryTracer { get; private set; }

        public void SetMainTracer<TTracer>() where TTracer : ITracer
        {
            SetMainTracer(typeof(TTracer));
        }

        public void SetMainTracer(Type tracer)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (!typeof(ITracer).GetTypeInfo().IsAssignableFrom(tracer.GetTypeInfo()))
                throw new ArgumentException($"'{tracer.GetType()}' does not implement '{nameof(ITracer)}'.", nameof(tracer));

            MainTracer = tracer;
        }

        public void SetSecondaryTracer<TTracer>() where TTracer : ITracer
        {
            SetSecondaryTracer(typeof(TTracer));
        }

        public void SetSecondaryTracer(Type tracer)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (!typeof(ITracer).GetTypeInfo().IsAssignableFrom(tracer.GetTypeInfo()))
                throw new ArgumentException($"'{tracer.GetType()}' does not implement '{nameof(ITracer)}'.", nameof(tracer));

            SecondaryTracer = tracer;
        }
    }
}