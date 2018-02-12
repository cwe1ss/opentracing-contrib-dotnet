namespace OpenTracing.Tracer
{
    public interface IPropagator
    {
        void Inject(ISpanContext context, object carrier);

        ISpanContext Extract(object carrier);
    }
}
