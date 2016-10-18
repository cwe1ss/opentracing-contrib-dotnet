using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Contrib;
using OpenTracing.Contrib.Http;
using OpenTracing.Contrib.ZipkinTracer;

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
            services.AddOpenTracing();

            // Send spans to Zipkin.
            services.AddZipkinTracer(options =>
            {
                options.ServiceName = "frontend";
            });

            services.AddSingleton(provider =>
            {
                var tracer = provider.GetRequiredService<ITracer>();
                var spanContextAccessor = provider.GetRequiredService<ISpanAccessor>();

                var openTracingHandler = new OpenTracingDelegatingHandler(tracer, spanContextAccessor);
                openTracingHandler.InnerHandler = new HttpClientHandler();

                return new HttpClient(openTracingHandler);
            });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseOpenTracing();

            app.UseMvcWithDefaultRoute();
        }
    }
}