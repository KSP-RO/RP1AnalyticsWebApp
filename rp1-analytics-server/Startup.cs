using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using rp1_analytics_server.Models;
using rp1_analytics_server.Services;

namespace rp1_analytics_server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CareerLogDatabaseSettings>(
                Configuration.GetSection(nameof(CareerLogDatabaseSettings)));

            services.AddSingleton<ICareerLogDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<CareerLogDatabaseSettings>>().Value);

            services.AddSingleton<CareerLogService>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(builder =>
            {
                builder.WithOrigins(
                    "http://localhost:8080");
            });
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}