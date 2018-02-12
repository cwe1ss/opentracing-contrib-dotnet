using Microsoft.AspNetCore.Http;

namespace OpenTracing.Instrumentation.AspNetCore.Mvc.Proxies
{
    public interface IActionContext
    {
        object ActionDescriptor { get; }
        HttpContext HttpContext { get; }
    }
}
