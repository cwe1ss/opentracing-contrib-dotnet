namespace OpenTracing.Contrib
{
    /// <summary>
    /// <para>Allows users to propagate a <see cref="ISpanContext"/> in-process by using
    /// either CallContext (net451) or AsyncLocal (netstandard1.3+) storage.</para>
    /// </summary>
    public interface ISpanContextAccessor
    {
        ISpanContext SpanContext { get; set;}
    }
}