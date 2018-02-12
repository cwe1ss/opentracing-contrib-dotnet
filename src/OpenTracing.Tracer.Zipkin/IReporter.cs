using System;

namespace OpenTracing.Tracer.Zipkin
{
    public interface IReporter : IDisposable
    {
        void Report(ISpan span);
    }
}
