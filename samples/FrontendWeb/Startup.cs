using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace Samples.FrontendWeb
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HttpClient>();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            ZipkinHelper.ConfigureZipkin(app);

            app.UseDeveloperExceptionPage();

            app.UseMvcWithDefaultRoute();
        }
    }
}
