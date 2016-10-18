namespace OpenTracing.Contrib.ZipkinTracer.Propagation
{
    public interface IPropagator
    {
        void Inject(SpanContext context, object carrier);

        SpanContext Extract(object carrier);
    }
}