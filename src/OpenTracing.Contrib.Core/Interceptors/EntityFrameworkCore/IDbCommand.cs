namespace OpenTracing.Contrib.Core.Interceptors.EntityFrameworkCore
{
    public interface IDbCommand
    {
        string CommandText { get; }
    }
}
