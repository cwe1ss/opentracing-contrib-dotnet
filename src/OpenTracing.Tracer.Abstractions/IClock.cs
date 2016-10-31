using System;

namespace OpenTracing.Tracer.Abstractions
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}