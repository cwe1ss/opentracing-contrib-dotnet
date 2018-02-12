using Microsoft.AspNetCore.Http;

namespace OpenTracing.Contrib.AspNetCore.Mvc.Proxies
{
    public interface IActionContext
    {
        object ActionDescriptor { get; }
        HttpContext HttpContext { get; }
    }
}
