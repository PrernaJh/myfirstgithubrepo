using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Processors;
using MMS.API.Domain.ZplUtilities;
using PackageTracker.AzureExtensions;
using PackageTracker.CosmosDbExtensions;
using PackageTracker.Data;
using PackageTracker.Data.Interfaces;
using PackageTracker.Domain;
using PackageTracker.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MMS.CreatePackage.API
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

            // Azure Service Bus
            var serviceBusEndpoint = configuration.GetConnectionString("ServiceBusEndpoint");
            var serviceBusTopicName = configuration.GetSection("ServiceBus").GetSection("TopicName").Value;
            services.AddServiceBusTopic(serviceBusEndpoint, serviceBusTopicName);

            //Add the ability to accept and validate Bearer tokens issued by authorized Okta domains
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                var audiences = configuration.GetSection("JwtSettings").GetSection("Audiences").Get<string[]>().ToList();
                cfg.SaveToken = true;
                cfg.Authority = configuration["JwtSettings:Issuer"];
                cfg.MetadataAddress = $"{configuration["JwtSettings:Issuer"]}/v2.0/.well-known/openid-configuration";
                cfg.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidAudiences = audiences,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true
                };
            });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // application insights
            services.AddApplicationInsightsTelemetry();

            // Add Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Mail Manifest System API",
                    Version = "v1",
                    Description = "Welcome to the Mail Manifest System API"
                });
                //Add the ability to use Bearer tokens in the Swagger UI
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = $"JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                //Add the ability to use Bearer tokens in the Swagger UI
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference
                          {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                          },
                          Scheme = "oauth2",
                          Name = "Bearer",
                          In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            services.AddMvcCore().AddApiExplorer();

            // repositories alphabetical
            services.AddScoped<IActiveGroupRepository, ActiveGroupRepository>();
            services.AddScoped<IBinRepository, BinRepository>();
            services.AddScoped<IContainerRepository, ContainerRepository>();
            services.AddScoped<IBinMapRepository, BinMapRepository>();
            services.AddScoped<IClientFacilityRepository, ClientFacilityRepository>();
            services.AddScoped<IFileConfigurationRepository, FileConfigurationRepository>();
            services.AddScoped<IOperationalContainerRepository, OperationalContainerRepository>();
            services.AddScoped<IPackageRepository, PackageRepository>();
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
            services.AddScoped<IClientFacilityProcessor, ClientFacilityProcessor>();
            services.AddScoped<IEodPostProcessor, EodPostProcessor>();
            services.AddScoped<IFileConfigurationProcessor, FileConfigurationProcessor>();
            services.AddScoped<ISequenceProcessor, SequenceProcessor>();
            services.AddScoped<ISiteProcessor, SiteProcessor>();
            services.AddScoped<ISubClientProcessor, SubClientProcessor>();
            services.AddScoped<IUspsShippingProcessor, UspsShippingProcessor>();
            services.AddScoped<IWebJobRunProcessor, WebJobRunProcessor>();
            services.AddScoped<IZipMapProcessor, ZipMapProcessor>();
            services.AddScoped<IZoneProcessor, ZoneProcessor>();

            // processors API only
            services.AddScoped<ICreatePackageProcessor, CreatePackageProcessor>();
            services.AddScoped<ICreatePackageServiceProcessor, CreatePackageServiceProcessor>();
            services.AddScoped<IOperationalContainerProcessor, OperationalContainerProcessor>();
            services.AddScoped<IPackageContainerProcessor, PackageContainerProcessor>();
            services.AddScoped<IPackageErrorProcessor, PackageErrorProcessor>();
            services.AddScoped<IPackageLabelProcessor, PackageLabelProcessor>();
            services.AddScoped<IServiceRuleExtensionScanProcessor, ServiceRuleExtensionScanProcessor>();
            services.AddScoped<ICreatePackageZplProcessor, CreatePackageZplProcessor>();

            services.BootstrapZPLProcessor(configuration);
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
            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("account", "{controller=Home}/{action=Index}");
            });

            //Only enable Swagger in non-production environments
            //if (env.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mail Manifest System Create Package API");
                c.RoutePrefix = string.Empty;
                c.InjectStylesheet("/swagger-css/flattop.css");
                c.DisplayRequestDuration();
            });
            //}

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var is64bit = 8 == IntPtr.Size;
                var logger = scope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger("MailManifestSystem.CreatePackageAPI.Startup");
                logger.LogInformation($"API Starting: Is 64 bit: {is64bit}");
                logger.LogInformation($"DatabaseName: {configuration.GetSection("DatabaseName").Value}");
            }
        }
    }
}
