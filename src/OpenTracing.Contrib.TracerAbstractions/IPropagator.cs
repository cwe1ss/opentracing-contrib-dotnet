namespace OpenTracing.Contrib.TracerAbstractions
{
    public interface IPropagator
    {
        void Inject(ISpanContext context, object carrier);

        ISpanContext Extract(object carrier);
    }
}