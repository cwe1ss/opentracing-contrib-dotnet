namespace OpenTracing.Contrib.TracerAbstractions
{
    public interface ISpanReporter
    {
        void ReportSpan(ISpan span);
    }
}