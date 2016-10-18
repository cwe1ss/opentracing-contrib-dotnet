using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Samples.CustomersApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseUrls(Constants.CustomersUrl)
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}