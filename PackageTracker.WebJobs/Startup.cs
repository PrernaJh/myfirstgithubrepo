using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PackageTracker.CosmosDbExtensions;
using System;

namespace PackageTracker.WebJobs
{
	public class Startup
	{
		private readonly IConfiguration configuration;

		public Startup(IWebHostEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables();

			configuration = builder.Build();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
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
			// application insights
			services.AddApplicationInsightsTelemetry();


			// dependency injection
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
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseStaticFiles();
			app.UseRouting();
			app.UseHttpsRedirection();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
			});
		}
	}
}
