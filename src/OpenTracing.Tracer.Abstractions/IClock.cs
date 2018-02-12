using System;

namespace OpenTracing.Tracer
{
    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }
}
