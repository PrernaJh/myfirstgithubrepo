using AutoMapper;
using DevExpress.AspNetCore;
using DevExpress.DashboardAspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using PackageTracker.AzureExtensions;
using PackageTracker.Communications;
using PackageTracker.Communications.Interfaces;
using PackageTracker.CosmosDbExtensions;
using PackageTracker.Data;
using PackageTracker.Data.Interfaces;
using PackageTracker.Domain;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Identity.Data.Models;
using PackageTracker.Identity.Service;
using PackageTracker.WebServices;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using ParcelPrepGov.Web.Infrastructure.Features;
using ParcelPrepGov.Web.Infrastructure.Middleware;
using ParcelPrepGov.Web.Models;
using System;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Repositories;
using ParcelPrepGov.Reports;
using System.Data;
using System.Data.SqlClient;
using PackageTracker.Domain.Utilities;
using Microsoft.Extensions.Logging;
using MMS.Web.Domain.Processors;
using MMS.Web.Domain.Interfaces;

namespace ParcelPrepGov.Web
{
    public class Startup
    {
        public readonly IConfiguration _configuration;
        public readonly IWebHostEnvironment _env;

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddHttpContextAccessor();
           
            services
                .AddDevExpressControls()
                .AddControllersWithViews(options =>
                {
                    options.Conventions.Add(new FeatureConvention());
                    options.Filters.Add(new AuthorizeFilter());
                })
                .AddRazorOptions(options =>
                {
                    options.ConfigureFeatureFolders();
                    options.ViewLocationFormats.Add("/{0}.cshtml");
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddDevExpressControls(settings => settings.Resources = ResourcesType.None);

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errorResponse = new ApiError(context.ModelState);
                    return new BadRequestObjectResult(errorResponse);
                };
            });

            // Redis Cache
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _configuration.GetConnectionString("Redis");
                options.InstanceName = "mms-pro-web-va-cache";
            });

            services.AddAutoMapper(typeof(Startup));

            // Bind CosmosDb database options. Invalid configuration will terminate the application startup.
            var cosmosDbConnectionStringOptions = _configuration.GetSection("ConnectionStrings").Get<ConnectionStringOptions>();
            var (serviceEndpoint, authKey) = cosmosDbConnectionStringOptions;
            var databaseName = _configuration.GetSection("DatabaseName").Value;

            // Verify CosmosDb database and collections exist
            services.AddCosmosDbToWeb(_configuration, serviceEndpoint, authKey, databaseName);
            services.AddBlobStorageAccount(_configuration.GetSection("AzureWebJobsStorage").Value);
            // Check CosmosDb connection. Cache result for 1 minute
            services.AddHealthChecks(checks =>
            {
                checks.AddCosmosDbCheck(serviceEndpoint, authKey, TimeSpan.FromMinutes(1));
            });

            // application insights
            services.AddApplicationInsightsTelemetry();

            var cloudStorageAccountSettings = _configuration.GetSection("AzureWebJobsStorage").Value;
            services.AddQueueStorageAccount(cloudStorageAccountSettings);

            // repositories cosmos
            services.AddScoped<IActiveGroupRepository, ActiveGroupRepository>();
            services.AddScoped<IBinRepository, BinRepository>();
            services.AddScoped<IBinMapRepository, BinMapRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IFileConfigurationRepository, FileConfigurationRepository>();
            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped<ISiteRepository, SiteRepository>();
            services.AddScoped<IServiceRuleRepository, ServiceRuleRepository>();
            services.AddScoped<IServiceRuleExtensionRepository, ServiceRuleExtensionRepository>();
            services.AddScoped<IZipOverrideRepository, ZipOverrideRepository>();
            services.AddScoped<IZipMapRepository, ZipMapRepository>();
            services.AddScoped<IZoneMapRepository, ZoneMapRepository>();
            services.AddScoped<IRateRepository, RateRepository>();
            services.AddScoped<ISubClientRepository, SubClientRepository>();
            services.AddScoped<IWebJobRunRepository, WebJobRunRepository>();

            // repositories reports
            services.AddScoped<IReportsRepository, ReportsRepository>();
            services.AddScoped<IPackageDatasetRepository, PackageDatasetRepository>();
            services.AddScoped<IPackageEventDatasetRepository, PackageEventDatasetRepository>();
            services.AddScoped<IPackageInquiryRepository, PackageInquiryRepository>();
            services.AddScoped<ITrackPackageDatasetRepository, TrackPackageDatasetRepository>();
            services.AddScoped<IVisnSiteRepository, VisnSiteRepository>();
            services.AddScoped<IBinDatasetRepository, BinDatasetRepository>();
            services.AddScoped<IShippingContainerDatasetRepository, ShippingContainerDatasetRepository>();
            services.AddScoped<IPostalAreaAndDistrictRepository, PostalAreaAndDistrictRepository>();
            services.AddScoped<IPostalDaysRepository, PostalDaysRepository>();
            services.AddScoped<ICarrierEventCodeRepository, CarrierEventCodeRepository>();
            services.AddScoped<IEvsCodeRepository, EvsCodeRepository>();
            services.AddScoped<IRecallStatusRepository, RecallStatusRepository>();
            services.AddScoped<IJobDatasetRepository, JobDatasetRepository>();
            services.AddScoped<IUserLookupRepository, UserLookupRepository>();
            services.AddScoped<IWebJobRunDatasetRepository, WebJobRunDatasetRepository>();

            // processors shared domain
            services.AddScoped<IActiveGroupProcessor, ActiveGroupProcessor>();
            services.AddScoped<IBinProcessor, BinProcessor>();
            services.AddScoped<IClientProcessor, ClientProcessor>();            
            services.AddScoped<IEodPostProcessor, EodPostProcessor>();
            services.AddScoped<IFileConfigurationProcessor, FileConfigurationProcessor>();
            services.AddScoped<IPackageDuplicateProcessor, PackageDuplicateProcessor>();
            services.AddScoped<IPackagePostProcessor, PackagePostProcessor>();            
            services.AddScoped<IRecallReleaseProcessor, RecallReleaseProcessor>();            
            services.AddScoped<ISiteProcessor, SiteProcessor>();
            services.AddScoped<ISubClientProcessor, SubClientProcessor>();
            services.AddScoped<IWebJobRunProcessor, WebJobRunProcessor>();
            services.AddScoped<IUserLookupProcessor, UserLookupProcessor>();

            // processors web domain
            services.AddScoped<IBinWebProcessor, BinWebProcessor>();
            services.AddScoped<IClientWebProcessor, ClientWebProcessor>();
            services.AddScoped<IGeoDescriptorsWebProcessor, GeoDescriptorsWebProcessor>();
            services.AddScoped<IRateWebProcessor, RateWebProcessor>();
            services.AddScoped<IServiceRuleExtensionWebProcessor, ServiceRuleExtensionWebProcessor>();
            services.AddScoped<IServiceRuleWebProcessor, ServiceRuleWebProcessor>();
            services.AddScoped<ISiteWebProcessor, SiteWebProcessor>();
            services.AddScoped<ISubClientWebProcessor, SubClientWebProcessor>();
            services.AddScoped<IZipOverrideWebProcessor, ZipOverrideWebProcessor>();
            services.AddScoped<IZoneMapsWebProcessor, ZoneMapsWebProcessor>();

            // processors reports
            services.AddScoped<IPackageSearchProcessor, PackageSearchProcessor>();

            // external clients
            services.AddScoped<IFedExTrackClient, FedExTrackApi.TrackPortTypeClient>();
            services.AddTransient<IDbConnection>((sp) => new SqlConnection(_configuration.GetConnectionString("PpgReportsDb")));

            // services
            services.AddTransient<IEmailService, EmailService>();
            services.AddSingleton<IEmailConfiguration>(_configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
            services.AddSingleton<IBlobHelper, BlobHelper>();

            services.BootstrapIdentityService(_configuration, _env);
            services.BootstrapDashboardAndReports(_configuration);

            services.AddAuthorization(options =>
            {
                options.AddWebPortalPolicies();
            });

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(1);
                //options.ExcludedHosts.Add("example.com");
                //options.ExcludedHosts.Add("www.example.com");
            });

            services.AddCors(OptionsBuilderConfigurationExtensions =>
            {
                OptionsBuilderConfigurationExtensions.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(origin => true) // allow any origin
                );
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // no harm with this executing in dev
                // uncomment to debug users, roles, or claims related issues
                // DataInitializer.SeedData(userManager, roleManager);
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //DevExpress.XtraReports.Configuration.Settings.Default.UserDesignerOptions.DataBindingMode = DevExpress.XtraReports.UI.DataBindingMode.Expressions;

            app.UseLoginExpirationCookie();
            app.UseDevExpressControls();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            //app.UseCors(policy => policy.WithHeaders(HeaderNames.CacheControl));
            app.UseCors("CorsPolicy");
            app.UseAuthorization();
            //app.UseAntiforgeryTokens();

            

            app.UseEndpoints(endpoints =>
            {
                EndpointRouteBuilderExtension.MapDashboardRoute(endpoints, "api/dashboards");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Landing}/{action=Index}/{id?}");
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var is64bit = 8 == IntPtr.Size;
                var logger = scope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger("ParcelPrepGov.Web.Startup");
                logger.LogInformation($"WEB Starting: Is 64 bit: {is64bit}");
                logger.LogInformation($"DatabaseName: {_configuration.GetSection("DatabaseName").Value}");
            }
        }

    }
}
