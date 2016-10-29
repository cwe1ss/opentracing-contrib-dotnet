using System;

namespace OpenTracing.Contrib.TracerAbstractions
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}