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
            services.AddOpenTracing()
                .AddAspNetCore();

            services.AddZipkinTracer(options =>
            {
                options.ServiceName = "customers";
            });

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
            BootstrapDataStore(app.ApplicationServices);

            // TODO @cweiss !!
            app.ApplicationServices.StartOpenTracing();

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