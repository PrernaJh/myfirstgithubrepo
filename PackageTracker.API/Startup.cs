using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Processors;
using MMS.API.Domain.ZplUtilities;
using PackageTracker.API.Infrastructure.Filters.JsonException;
using PackageTracker.Communications;
using PackageTracker.Communications.Interfaces;
using PackageTracker.CosmosDbExtensions;
using PackageTracker.Data;
using PackageTracker.Data.Interfaces;
using PackageTracker.Domain;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Identity.Data;
using PackageTracker.Identity.Data.Models;
using PackageTracker.WebServices;
using System;

namespace PackageTracker.API
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            configuration = builder.Build();
        }

        // Called by the runtime. Use to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddRazorPages();

            // Add DbContext for Entity Framework and configure context to connect to the Identity db
            services.AddDbContext<PackageTrackerIdentityDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("IdentityDb")));

            // Bind CosmosDb database options. Invalid configuration will terminate the application startup.
            var cosmosDbConnectionStringOptions = configuration.GetSection("ConnectionStrings").Get<ConnectionStringOptions>();
            var (serviceEndpoint, authKey) = cosmosDbConnectionStringOptions;
            var databaseName = configuration.GetSection("DatabaseName").Value;

			// Verify CosmosDb database and collections exist
			services.AddCosmosDbToApi(configuration, serviceEndpoint, authKey, databaseName);
			var timeout = TimeSpan.FromSeconds(3);
			// Check CosmosDb connection. Cache result for 1 minute
			services.AddHealthChecks(checks =>
			{
				checks.AddCosmosDbCheck(serviceEndpoint, authKey, TimeSpan.FromMinutes(1));
			});

            // Add and configure Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options => IdentityOptionsHelper.SetIdentityOptions(options))
                .AddEntityFrameworkStores<PackageTrackerIdentityDbContext>()
                .AddDefaultTokenProviders();

            //.AddRoles<IdentityRole>()
            //.AddPasswordValidator<ApplicationUser>();
            //.AddRoleManager<IdentityRole>();

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.Configure<PasswordHasherOptions>(option => // TODO: Do we need to force logout for the API if user is deactivated?
            {
                option.IterationCount = 12000;
            });

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromSeconds(60);
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Name = "ppgpro_api";
            });

            // application insights
            services.AddApplicationInsightsTelemetry();

            // Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ParcelPrepGov API",
                    Version = "v1",
                    Description = "Welcome to the ParcelPrepGov API"
                });
            });

            services.AddMvcCore(options =>
            {
                options.Filters.Add<JsonExceptionFilter>();
            }).AddApiExplorer();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errorResponse = new ApiError(context.ModelState);
                    return new BadRequestObjectResult(errorResponse);
                };
            });

            //services.AddCors();

            // UPS API client configs
            var upsShipEndpoint = configuration.GetSection("UpsApis").GetSection("ShipEndpointUri").Value;
            var upsShipClient = new UpsShipApi.ShipPortTypeClient(0, upsShipEndpoint);
            var upsVoidEndpoint = configuration.GetSection("UpsApis").GetSection("VoidEndpointUri").Value;
            var upsVoidClient = new UpsVoidApi.VoidPortTypeClient(0, upsVoidEndpoint);

            // FedEx Ship API client configs
            var fedExShipEndpoint = configuration.GetSection("FedExApis").GetSection("ShipEndpointUri").Value;
            var fedExShipClient = new FedExShipApi.ShipPortTypeClient(0, fedExShipEndpoint);

            // repositories alphabetical
            services.AddScoped<IActiveGroupRepository, ActiveGroupRepository>();
            services.AddScoped<IBinRepository, BinRepository>();
            services.AddScoped<IBinMapRepository, BinMapRepository>();
            services.AddScoped<IContainerRepository, ContainerRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IClientFacilityRepository, ClientFacilityRepository>();
            services.AddScoped<IFileConfigurationRepository, FileConfigurationRepository>();
            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<IJobOptionRepository, JobOptionRepository>();
            services.AddScoped<IOperationalContainerRepository, OperationalContainerRepository>();
            services.AddScoped<IReturnOptionRepository, ReturnOptionRepository>();
            services.AddScoped<ISequenceRepository, SequenceRepository>();
            services.AddScoped<IServiceRuleRepository, ServiceRuleRepository>();
            services.AddScoped<IServiceRuleExtensionRepository, ServiceRuleExtensionRepository>();
            services.AddScoped<ISiteRepository, SiteRepository>();
            services.AddScoped<ISubClientRepository, SubClientRepository>();
            services.AddScoped<IWebJobRunRepository, WebJobRunRepository>();
            services.AddScoped<IZipMapRepository, ZipMapRepository>();
            services.AddScoped<IZipOverrideRepository, ZipOverrideRepository>();
            services.AddScoped<IZoneMapRepository, ZoneMapRepository>();

            // processors shared
            services.AddScoped<IActiveGroupProcessor, ActiveGroupProcessor>();

            services.AddScoped<IBinProcessor, BinProcessor>();
            services.AddScoped<IClientProcessor, ClientProcessor>();
            services.AddScoped<IClientFacilityProcessor, ClientFacilityProcessor>();
            services.AddScoped<IEodPostProcessor, EodPostProcessor>();
            services.AddScoped<IFileConfigurationProcessor, FileConfigurationProcessor>();
            services.AddScoped<IPackageDuplicateProcessor, PackageDuplicateProcessor>();
            services.AddScoped<IPackagePostProcessor, PackagePostProcessor>();
            services.AddScoped<IPackagePostProcessor, PackagePostProcessor>();
            services.AddScoped<IReturnProcessor, ReturnProcessor>();
            services.AddScoped<ISequenceProcessor, SequenceProcessor>();
            services.AddScoped<IShippingProcessor, ShippingProcessor>();
            services.AddScoped<ISiteProcessor, SiteProcessor>();
            services.AddScoped<ISubClientProcessor, SubClientProcessor>();
            services.AddScoped<IUspsShippingProcessor, UspsShippingProcessor>();
            services.AddScoped<IWebJobRunProcessor, WebJobRunProcessor>();
            services.AddScoped<IZipMapProcessor, ZipMapProcessor>();
            services.AddScoped<IZoneProcessor, ZoneProcessor>();
            services.AddScoped<IServiceRuleProcessor, ServiceRuleProcessor>();

            // processors API only

            services.AddScoped<IAutoScanProcessor, AutoScanProcessor>();
            services.AddScoped<IContainerProcessor, ContainerProcessor>();
            services.AddScoped<IContainerBarcodeProcessor, ContainerBarcodeProcessor>();
            services.AddScoped<IContainerLabelProcessor, ContainerLabelProcessor>();
            services.AddScoped<IContainerUpdateProcessor, ContainerUpdateProcessor>();
            services.AddScoped<IContainerShippingProcessor, ContainerShippingProcessor>();
            services.AddScoped<ICreatePackageScanProcessor, CreatePackageScanProcessor>();
            services.AddScoped<ICreatePackageServiceProcessor, CreatePackageServiceProcessor>();
            services.AddScoped<IJobScanProcessor, JobScanProcessor>();
            services.AddScoped<IOperationalContainerProcessor, OperationalContainerProcessor>();
            services.AddScoped<IPackageContainerProcessor, PackageContainerProcessor>();
            services.AddScoped<IPackageErrorProcessor, PackageErrorProcessor>();
            services.AddScoped<IPackageLabelProcessor, PackageLabelProcessor>();
            services.AddScoped<IPackageRepeatProcessor, PackageRepeatProcessor>();
            services.AddScoped<IPackageScanProcessor, PackageScanProcessor>();
            services.AddScoped<IPackageServiceProcessor, PackageServiceProcessor>();
            services.AddScoped<IServiceRuleExtensionScanProcessor, ServiceRuleExtensionScanProcessor>();
            services.AddScoped<ICreatePackageProcessor, CreatePackageProcessor>();
            services.AddScoped<ICreatePackageZplProcessor, CreatePackageZplProcessor>();

            // Email 
            var emailConfig = configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
            if (emailConfig != null)
            {
                services.AddScoped<IEmailService, EmailService>();
                services.AddSingleton<IEmailConfiguration>(emailConfig);
            }

            services.BootstrapZPLProcessor(configuration);

            // web services
            services.AddSingleton<IFedExShipClient>(fedExShipClient);
            services.AddSingleton<IUpsShipClient>(upsShipClient);
            services.AddSingleton<IUpsVoidClient>(upsVoidClient);

        }

        // Called by the runtime. Use to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            //app.UseCors(builder => builder.WithOrigins("https://api.parcelprepgov", "https://ppg-pro-appservice-va-api", "https://ppg-pro-appservice-tx-api")
            //                    .AllowAnyMethod()
            //                    .AllowAnyHeader());
            app.UseHttpsRedirection();
            app.UseCookiePolicy();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapHealthChecks("/health");
                endpoints.MapControllerRoute("account", "{controller=Home}/{action=Index}");
            });

            //Only enable Swagger in non-production environments
            //if (env.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParcelPrepGov API");
                c.RoutePrefix = string.Empty;
                c.InjectStylesheet("/swagger-css/flattop.css");
                c.DisplayRequestDuration();
            });
            //}

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var is64bit = 8 == IntPtr.Size;
                var logger = scope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger("PackageTracker.API.Startup");
                logger.LogInformation($"API Starting: Is 64 bit: {is64bit}");
                logger.LogInformation($"DatabaseName: {configuration.GetSection("DatabaseName").Value}");
            }
        }
    }
}