namespace OpenTracing.Tracer.Abstractions
{
    public interface IPropagator
    {
        void Inject(ISpanContext context, object carrier);

        ISpanContext Extract(object carrier);
    }
}