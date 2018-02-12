using System;

namespace OpenTracing.Contrib
{
    public interface IDiagnosticInterceptor : IDisposable
    {
        void Start();
    }
}
