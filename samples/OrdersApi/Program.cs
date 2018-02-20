using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Shared;

namespace Samples.OrdersApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(Constants.OrdersUrl)
                .UseOpenTracing() // Enables OpenTracing instrumentation for ASP.NET Core, HttpClient, EF Core
                .Build();
        }
    }
}
