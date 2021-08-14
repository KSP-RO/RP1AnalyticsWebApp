using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
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
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder("Admin", "/", "RequireAdministratorRole");
            })
            .AddRazorRuntimeCompilation();

            services.Configure<CareerLogDatabaseSettings>(
                Configuration.GetSection(nameof(CareerLogDatabaseSettings)));

            services.AddSingleton<ICareerLogDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<CareerLogDatabaseSettings>>().Value);

            services.AddSingleton<IContractSettings>(sp =>
                sp.GetRequiredService<IOptions<ContractSettings>>().Value);

            services.AddSingleton<ITechTreeSettings>(sp =>
                sp.GetRequiredService<IOptions<TechTreeSettings>>().Value);

            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("CustomConventions", pack,
               t => t.FullName.StartsWith("RP1AnalyticsWebApp.Models."));

            services.AddTransient<CareerLogService>();

            services.AddIdentityMongoDbProvider<WebAppUser, MongoRole>(identityOptions =>
            {
            },
            mongoIdentityOptions =>
            {
                mongoIdentityOptions.ConnectionString = Configuration["CareerLogDatabaseSettings:ConnectionString"];
            });

            services.AddControllers();
            services.AddRazorPages();
            services.AddApplicationInsightsTelemetry();
            services.AddSwaggerGen();
            services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();

            services.AddAuthentication()
                .AddGitHub(options =>
                {
                    options.ClientId = Configuration["Authentication:GitHub:ClientId"];
                    options.ClientSecret = Configuration["Authentication:GitHub:ClientSecret"];
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdministratorRole",
                     policy => policy.RequireRole(Constants.Roles.Admin));
            });

            services.AddHostedService<StartupHostedService>();
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