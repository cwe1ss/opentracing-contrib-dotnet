namespace OpenTracing.Tracer.Zipkin.Reporter
{
    public interface ISpanReporter
    {
        void ReportSpan(ZipkinSpan span);
    }
}