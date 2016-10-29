namespace OpenTracing.Contrib.ZipkinTracer.Reporter
{
    public interface ISpanReporter
    {
        void ReportSpan(ZipkinSpan span);
    }
}