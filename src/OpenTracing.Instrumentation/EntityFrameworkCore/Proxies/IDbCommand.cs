namespace OpenTracing.Instrumentation.EntityFrameworkCore.Proxies
{
    public interface IDbCommand
    {
        string CommandText { get; }
    }
}
