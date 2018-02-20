using System;

namespace OpenTracing.Contrib.AspNetCore
{
    public interface IDiagnosticInterceptor : IDisposable
    {
        void Start();
    }
}
