using System;

namespace OpenTracing.Tracer.Abstractions
{
    public interface IReporter : IDisposable
    {
        void Report(ISpan span);
    }
}