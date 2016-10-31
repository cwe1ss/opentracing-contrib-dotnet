using System;

namespace OpenTracing.Instrumentation
{
    /// <summary>
    /// <para>Allows users to propagate spans with its <see cref="ISpanContext"/> in-process by using
    /// either CallContext (net451) or AsyncLocal (netstandard1.3+) storage.</para>
    /// </summary>
    public interface ITraceContext
    {
        /// <summary>
        /// Returns the number of spans on the local execution storage.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the latest span from the local execution storage without removing it.
        /// </summary>
        ISpan CurrentSpan { get; }

        /// <summary>
        /// Returns wheter or not there are spans on the local execution storage.
        /// </summary>
        bool IsEmpty();

        /// <summary>
        /// Adds the given span to the local execution storage.
        /// </summary>
        /// <returns>A disposable which calls <see cref="TryPop"/> on the stack when it is disposed.</returns>
        IDisposable Push(ISpan span);

        /// <summary>
        /// Removes and returns the lastest span from the local execution storage.
        /// Returns <c>null</c> if the stack is empty.
        /// </summary>
        ISpan TryPop();
    }
}