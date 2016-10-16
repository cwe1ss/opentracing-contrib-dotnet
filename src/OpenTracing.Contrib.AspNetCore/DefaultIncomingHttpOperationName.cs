using Microsoft.AspNetCore.Http;

namespace OpenTracing.Contrib.AspNetCore
{
    public class DefaultIncomingHttpOperationName : IIncomingHttpOperationName
    {
        public string GetOperationName(HttpRequest request)
        {
            return request.Path;
        }
    }
}