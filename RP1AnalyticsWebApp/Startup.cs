using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.MediaType;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using MongoDB.Bson.Serialization.Conventions;
using RP1AnalyticsWebApp.Models;
using RP1AnalyticsWebApp.OData;
using RP1AnalyticsWebApp.Services;
using System.Linq;

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

            services.Configure<AppSettings>(
                Configuration.GetSection(nameof(AppSettings)));

            services.AddSingleton<ICareerLogDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<CareerLogDatabaseSettings>>().Value);

            services.AddSingleton<IAppSettings>(sp =>
                sp.GetRequiredService<IOptions<AppSettings>>().Value);

            services.AddSingleton<IContractSettings>(sp =>
                sp.GetRequiredService<IOptions<ContractSettings>>().Value);

            services.AddSingleton<ITechTreeSettings>(sp =>
                sp.GetRequiredService<IOptions<TechTreeSettings>>().Value);

            services.AddSingleton<IProgramSettings>(sp =>
                sp.GetRequiredService<IOptions<ProgramSettings>>().Value);

            services.AddSingleton<ILeaderSettings>(sp =>
                sp.GetRequiredService<IOptions<LeaderSettings>>().Value);

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

            services.AddControllers()
                    .AddOData(opt => opt.AddRouteComponents(
                        "odata", GetEdmModel(),
                        service => service.AddSingleton<ODataMediaTypeResolver>(sp => new CsvMediaTypeResolver()))
                    .Filter());

            services.AddControllers(opt =>
            {
                var odataFormatter = opt.OutputFormatters.OfType<ODataOutputFormatter>().First();
                odataFormatter.SupportedMediaTypes.Add("text/csv");
                odataFormatter.MediaTypeMappings.Add(new QueryStringMediaTypeMapping("$format", "csv", "text/csv"));
            });

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

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
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

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            var careerLog = builder.EntityType<CareerLog>();
            careerLog.HasMany(c => c.CareerLogEntries);
            careerLog.HasMany(c => c.LaunchEventEntries);
            //careerLog.HasMany(c => c.ContractEventEntries);
            //careerLog.HasMany(c => c.FacilityEventEntries);
            //careerLog.HasMany(c => c.TechEventEntries);

            var clp = builder.EntityType<CareerLogPeriod>();
            clp.HasKey(o => o.StartDate);

            var le = builder.EntityType<LaunchEvent>();
            le.HasKey(o => o.LaunchID);

            var cr = builder.EntityType<ContractRecord>();
            cr.HasKey(o => o.CareerId);

            var pr = builder.EntityType<ProgramRecord>();
            pr.HasKey(o => o.CareerId);

            var pi = builder.EntityType<ProgramItemWithCareerInfo>();
            pi.HasKey(o => o.Name);

            var ce = builder.EntityType<ContractEventWithCareerInfo>();
            ce.HasKey(o => o.ContractInternalName);


            builder.EntitySet<CareerLog>("careers");

            builder.EntitySet<CareerLogPeriod>("careerLogEntries");

            builder.EntitySet<CareerListItem>("careerListItems");

            builder.EntitySet<ContractRecord>("contractRecords");

            builder.EntitySet<ProgramRecord>("programRecords");

            builder.EntitySet<ProgramItemWithCareerInfo>("programs");

            builder.EntitySet<ContractEventWithCareerInfo>("contracts");

            builder.EnableLowerCamelCase();

            return builder.GetEdmModel();
        }
    }
}