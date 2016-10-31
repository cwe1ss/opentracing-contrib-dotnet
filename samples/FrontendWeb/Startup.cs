using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Samples.FrontendWeb
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Allows this application to create spans.
            services.AddOpenTracing()
                .AddAspNetCore();

            // Send spans to Zipkin.
            services.AddZipkinTracer(options =>
            {
                options.ServiceName = "frontend";
            });

            services.AddSingleton<HttpClient>();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            // TODO @cweiss !!
            app.ApplicationServices.StartOpenTracing();

            app.UseDeveloperExceptionPage();

            app.UseMvcWithDefaultRoute();
        }
    }
}