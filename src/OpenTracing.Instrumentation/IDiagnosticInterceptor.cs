using System;

namespace OpenTracing.Instrumentation
{
    public interface IDiagnosticInterceptor : IDisposable
    {
        void Start();
    }
}
