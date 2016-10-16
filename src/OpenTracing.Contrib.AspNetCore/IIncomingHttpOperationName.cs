using Microsoft.AspNetCore.Http;

namespace OpenTracing.Contrib.AspNetCore
{
    public interface IIncomingHttpOperationName
    {
        string GetOperationName(HttpRequest request);
    }
}