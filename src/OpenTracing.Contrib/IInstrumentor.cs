using System;

namespace OpenTracing.Contrib
{
    public interface IInstrumentor : IDisposable
    {
        void Start();
    }
}
