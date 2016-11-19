using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samples.CustomersApi.DataStore;

namespace Samples.CustomersApi
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
            // Enable OpenTracing instrumentation
            services.AddInstrumentation()   // Adds default instrumentations (HttpClient, EFCore)
                .AddAspNetCore();           // Adds ASP.NET Core request instrumentation and auto-starts instrumentation

            // Send traces to Zipkin
            services.AddZipkinTracer(options => options
                .WithZipkinUri("http://localhost:9411")
                .WithServiceName("customers"));

            // Adds an InMemory-Sqlite DB to show EFCore traces.
            services
                .AddEntityFrameworkSqlite()
                .AddDbContext<CustomerDbContext>(options =>
                {
                    var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
                    var connectionString = connectionStringBuilder.ToString();
                    var connection = new SqliteConnection(connectionString);

                    options.UseSqlite(connection);
                });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            // Load some dummy data into the InMemory db.
            BootstrapDataStore(app.ApplicationServices);

            app.UseDeveloperExceptionPage();

            app.UseMvc();
        }

        public void BootstrapDataStore(IServiceProvider serviceProvider)
        {
            var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
                dbContext.Seed();
            }
        }
    }
}