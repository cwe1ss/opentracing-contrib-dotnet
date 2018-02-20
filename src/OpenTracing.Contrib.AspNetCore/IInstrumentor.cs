using System;

namespace OpenTracing.Contrib.AspNetCore
{
    public interface IInstrumentor : IDisposable
    {
        void Start();
    }
}
