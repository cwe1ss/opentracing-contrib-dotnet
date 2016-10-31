using System;

namespace OpenTracing.Instrumentation
{
    public interface IInstrumentor : IDisposable
    {
        void Start();
    }
}