namespace OpenTracing.Contrib.AspNetCore.Interceptors.EntityFrameworkCore
{
    public interface IDbCommand
    {
        string CommandText { get; }
    }
}
