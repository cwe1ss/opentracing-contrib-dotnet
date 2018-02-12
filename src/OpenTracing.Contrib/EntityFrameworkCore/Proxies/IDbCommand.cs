namespace OpenTracing.Contrib.EntityFrameworkCore.Proxies
{
    public interface IDbCommand
    {
        string CommandText { get; }
    }
}
