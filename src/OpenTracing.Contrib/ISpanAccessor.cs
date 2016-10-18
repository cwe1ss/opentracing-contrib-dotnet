namespace OpenTracing.Contrib
{
    /// <summary>
    /// <para>Allows users to propagate a span with its <see cref="ISpanContext"/> in-process by using
    /// either CallContext (net451) or AsyncLocal (netstandard1.3+) storage.</para>
    /// </summary>
    public interface ISpanAccessor
    {
        ISpan Span { get; set;}
    }
}