namespace OpenTracing.Contrib.ZipkinTracer.Reporter
{
    public interface IReporter
    {
        void Report(Span span);
    }
}