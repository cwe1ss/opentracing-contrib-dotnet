using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseOpenTracing(this IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddOpenTracing();
            });

            return builder;
        }
    }
}
