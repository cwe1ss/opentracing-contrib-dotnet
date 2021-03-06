using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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

                // Enables OpenTracing instrumentation for ASP.NET Core, HttpClient, EF Core
                .UseOpenTracing()

                // Register and start Zipkin
                .ConfigureServices(services => services.AddSingleton<ZipkinManager>())
                .Configure(app => app.ApplicationServices.GetRequiredService<ZipkinManager>().Start())

                .Build();
        }
    }
}
