using Microsoft.AspNetCore.Http;

namespace OpenTracing.Contrib.AspNetCore
{
    public static class HttpContextExtensions
    {
        public static ISpan GetCurrentSpan(this HttpContext context)
        {
            return (ISpan)context.Items[typeof(ISpan)];
        }
    }
}