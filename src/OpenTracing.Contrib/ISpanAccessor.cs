using System;

namespace OpenTracing.Contrib
{
    /// <summary>
    /// <para>Allows users to propagate spans with its <see cref="ISpanContext"/> in-process by using
    /// either CallContext (net451) or AsyncLocal (netstandard1.3+) storage.</para>
    /// </summary>
    public interface ISpanAccessor
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
        /// Adds the given span to the local execution storage.
        /// </summary>
        /// <returns>A disposable which pops the given span from the stack.</returns>
        IDisposable Push(ISpan span);

        /// <summary>
        /// Removes and returns the lastest span from the local execution storage.
        /// Returns <c>null</c> if the stack is empty.
        /// </summary>
        ISpan TryPop();
    }
}