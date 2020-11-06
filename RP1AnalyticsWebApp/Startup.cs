using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.Services;

namespace RP1AnalyticsWebApp
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

            services.AddSingleton<IContractSettings>(sp =>
                sp.GetRequiredService<IOptions<ContractSettings>>().Value);

            services.AddSingleton<CareerLogService>();

            services.AddIdentityMongoDbProvider<MongoUser, MongoRole>(identityOptions =>
            {
            }, mongoIdentityOptions => {
                mongoIdentityOptions.ConnectionString = Configuration["CareerLogDatabaseSettings:ConnectionString"];
            });

            services.AddControllers();
            services.AddRazorPages();
            services.AddApplicationInsightsTelemetry();
            services.AddSwaggerGen();

            services.AddAuthentication()
                .AddGitHub(options =>
                {
                    options.ClientId = Configuration["Authentication:GitHub:ClientId"];
                    options.ClientSecret = Configuration["Authentication:GitHub:ClientSecret"];
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 API");
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors(builder =>
            {
                builder.WithOrigins(
                    "http://localhost:8080");
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}