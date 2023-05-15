using DevExpress.DashboardAspNetCore;
using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using Microsoft.Extensions.DependencyInjection;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Reports.Data;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Repositories;
using ParcelPrepGov.Web.Features.Dashboard.Data;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace ParcelPrepGov.Web.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
	{
		public static IServiceCollection BootstrapDashboardAndReports(this IServiceCollection services, IConfiguration _configuration)
		{
			services.AddDbContext<PpgReportsDbContext>();

			services.AddSingleton<IPpgReportsDbContextFactory, PpgReportsDbContextFactory>();

			services.AddScoped<IPpgDashboardStorage, DashboardStorage>();

			services.AddControllersWithViews().AddDefaultDashboardController((configurator, serviceProvider) =>
			{
				Dictionary<string, string> connectionStrings = new Dictionary<string, string>();
				
				connectionStrings.Add("ConnectionStrings:PpgReportsDb", _configuration.GetConnectionString("PpgReportsDb"));			

				var builder = new ConfigurationBuilder().AddInMemoryCollection(connectionStrings);

				var configuration = builder.Build();

				configurator.SetConnectionStringsProvider(new DashboardConnectionStringsProvider(configuration));

				configurator.SetDashboardStorage(new DashboardStorage(serviceProvider));
				configurator.SetDataSourceStorage(CreateDataSourceStorage());


				configurator.DataLoading += (s, e) =>
				{
					if (e.DataSourceName == DataConstants.PACKAGES)
					{
						e.Data = PackagesData.Execute(serviceProvider);
					}
				};

			});

			services.AddScoped<IReportingRepository, ReportingRepository>();						
			//services.AddScoped<IUserService, UserService>();
			//services.AddScoped<IWebDocumentViewerReportResolver, WebDocumentViewerReportResolver>();
			//services.AddScoped<IObjectDataSourceInjector, ObjectDataSourceInjector>();
			//services.AddScoped<PreviewReportCustomizationService, CustomPreviewReportCustomizationService>();
			//services.AddTransient<PackageTrackerReportRepository>();
			//services.AddTransient<ReportDataRepository>(); 
			//services.AddScoped<PreviewReportCustomizationService, CustomPreviewReportCustomizationService>();

			//Throws error when unauthorized
			//services.AddScoped<IWebDocumentViewerAuthorizationService, DocumentViewerAuthorizationService>();
			//services.AddScoped<WebDocumentViewerOperationLogger, DocumentViewerAuthorizationService>();

			//services.AddScoped<ReportStorageWebExtension, CustomReportStorageWebExtension>();

			return services;
		}

		private static DataSourceInMemoryStorage CreateDataSourceStorage()
		{
			DataSourceInMemoryStorage dataSourceStorage = new DataSourceInMemoryStorage();

			DashboardObjectDataSource objDataSource = new DashboardObjectDataSource(DataConstants.PACKAGES);
			dataSourceStorage.RegisterDataSource(DataConstants.PACKAGES, objDataSource.SaveToXml());

			return dataSourceStorage;
		}

		public static void AddWebPortalPolicies(this Microsoft.AspNetCore.Authorization.AuthorizationOptions options)
		{
			options.AddPolicy(PPGClaim.WebPortal.Policy.UserManager,
					policy => policy.RequireAssertion(context => context.User.HasClaim(c => (PPGClaim.WebPortal.UserManagement.GetUserManagement().Contains(c.Type)))));

			options.AddPolicy(PPGClaim.WebPortal.Policy.Dashboard,
					policy => policy.RequireAssertion(context => context.User.HasClaim(c => (PPGClaim.WebPortal.Dashboard.GetAllDashboardClaims().Contains(c.Type)))));

			options.AddPolicy(PPGClaim.WebPortal.Policy.Reporting,
					policy => policy.RequireAssertion(context => context.User.HasClaim(c => (PPGClaim.WebPortal.Reporting.GetAllReportingClaims().Contains(c.Type)))));

			options.AddPolicy(PPGClaim.WebPortal.Policy.ServiceManagement,
					policy => policy.RequireAssertion(context => context.User.HasClaim(c => (PPGClaim.WebPortal.ServiceManagement.GetBaseServiceManagementClaims().Contains(c.Type)))));


			options.AddPolicy(PPGClaim.WebPortal.UserManagement.AddSubClientAdmin, policy => policy.RequireClaim(PPGClaim.WebPortal.UserManagement.AddSubClientAdmin));
			options.AddPolicy(PPGClaim.WebPortal.UserManagement.AddSiteUser,
				policy => policy.RequireAssertion(context => context.User.HasClaim(c => (PPGClaim.WebPortal.UserManagement.GetSiteUserManagement().Contains(c.Type)))));

			
			options.AddPolicy(PPGClaim.WebPortal.UserManagement.AddFSCWebFinancialUser, policy => policy.RequireClaim(PPGClaim.WebPortal.UserManagement.AddFSCWebFinancialUser));

			options.AddPolicy(PPGClaim.WebPortal.UserManagement.AddTransportationUser, policy => policy.RequireClaim(PPGClaim.WebPortal.UserManagement.AddTransportationUser));

			options.AddPolicy(PPGClaim.WebPortal.UserManagement.AddCustomerServiceUser, policy => policy.RequireClaim(PPGClaim.WebPortal.UserManagement.AddCustomerServiceUser));


			List<string> claimPolicies = new List<string>
			{
				PPGClaim.WebPortal.FileManagement.EndOfDayFiles,
				PPGClaim.WebPortal.FileManagement.AsnImports,
				PPGClaim.WebPortal.FileManagement.AzureBlob,
				PPGClaim.WebPortal.FileManagement.ProgramManagementBlob,
				PPGClaim.WebPortal.ServiceManagement.ManageBinRules,
				PPGClaim.WebPortal.ServiceManagement.ManageCostsAndCharges,
				PPGClaim.WebPortal.ServiceManagement.ManageGeoDescriptors,
				PPGClaim.WebPortal.ServiceManagement.ManageServiceRules,
				PPGClaim.WebPortal.ServiceManagement.ManageSiteAlerts,
				PPGClaim.WebPortal.ServiceManagement.ManageExtendedServiceRules,
				PPGClaim.WebPortal.ServiceManagement.ManageZipSchemas,
				PPGClaim.WebPortal.ServiceManagement.ManageZoneMaps,
				PPGClaim.WebPortal.ServiceManagement.ManageUspsRegions,
				PPGClaim.WebPortal.ServiceManagement.ManageUspsHolidays,
				PPGClaim.WebPortal.ServiceManagement.ManageUspsEvsCodes,
				PPGClaim.WebPortal.ServiceManagement.ManageUspsVisnSite,
				PPGClaim.WebPortal.ServiceManagement.ServiceOverride,
				PPGClaim.WebPortal.ContainerManagement.ContainerSearch,
				PPGClaim.WebPortal.PackageManagement.ReadPackageRecall,
				PPGClaim.WebPortal.PackageManagement.ReadPackageRelease,
				PPGClaim.WebPortal.PackageManagement.PackageSearch,
				PPGClaim.WebPortal.PackageManagement.PackageRecall,
				PPGClaim.WebPortal.PackageManagement.PackageRelease,
				PPGClaim.WebPortal.PackageManagement.DeleteRecallPackage,
				PPGClaim.WebPortal.Dashboard.AllSites,
				PPGClaim.WebPortal.Dashboard.SiteSpecific,
				PPGClaim.WebPortal.Reporting.Admin,
				PPGClaim.WebPortal.Reporting.SiteSpecific,
				PPGClaim.WebPortal.Reporting.ClientSpecific,
				PPGClaim.WebPortal.Reporting.SubClientSpecific
			};

			foreach (var claimPolicy in claimPolicies)
			{
				options.AddPolicy(claimPolicy, policy => policy.RequireClaim(claimPolicy));
			}

			options.AddPolicy(PPGClaim.WebPortal.UserManagement.AddClientUser,
				policy => policy.RequireAssertion(context => context.User.HasClaim(c => c.Type == PPGClaim.WebPortal.UserManagement.AddClientUser || c.Type == PPGClaim.WebPortal.UserManagement.AddClientAdmin)));
			options.AddPolicy(PPGClaim.WebPortal.UserManagement.AddClientPackageSearchUser,
				policy => policy.RequireAssertion(context => context.User.HasClaim(c => c.Type == PPGClaim.WebPortal.UserManagement.AddClientPackageSearchUser || c.Type == PPGClaim.WebPortal.UserManagement.AddClientAdmin)));
			options.AddPolicy(PPGClaim.WebPortal.UserManagement.AddSubClientUser,
				policy => policy.RequireAssertion(context => context.User.HasClaim(c => c.Type == PPGClaim.WebPortal.UserManagement.AddSubClientUser || c.Type == PPGClaim.WebPortal.UserManagement.AddSubClientAdmin)));
		}
	}
}
