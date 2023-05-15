using System.Collections.Generic;

namespace PackageTracker.Identity.Data.Constants
{
	public class PPGClaim
	{
		public const string Site = "SITE";
		public const string Client = "CLIENT";
		public const string SubClient = "SUBCLIENT";

		public static class WebPortal
		{
			public static class Policy
			{
				public const string UserManager = "WebPortal.Policy.UserManager";
				public const string SiteUserManager = "WebPortal.Policy.SiteUserManager";
				public const string ClientUserManager = "WebPortal.Policy.ClientUserManager";
				public const string SubClientUserManager = "WebPortal.Policy.SubClientUserManager";
				public const string Dashboard = "WebPortal.Policy.Dashboard";
				public const string Reporting = "WebPortal.Policy.Reporting";
				public const string ServiceManagement = "WebPortal.Policy.ServiceManagement";
			}

			public static class UserManagement
			{
				public const string AddSystemAdmin = "WebPortal.UserManagement.AddSystemAdmin";
				public const string AddClientAdmin = "WebPortal.UserManagement.AddClientAdmin";
				public const string AddClientUser = "WebPortal.UserManagement.AddClientUser";
				public const string AddClientPackageSearchUser = "WebPortal.UserManagement.AddClientPackageSearchUser";
				public const string AddSubClientAdmin = "WebPortal.UserManagement.AddSubClientAdmin";
				public const string AddSubClientUser = "WebPortal.UserManagement.AddSubClientUser";
				public const string AddAdministrator = "WebPortal.UserManagement.AddAdministrator";
				public const string AddSiteUser = "WebPortal.UserManagement.AddSiteUser";
				public const string AddGeneralManager = "WebPortal.UserManagement.AddGeneralManager";
				public const string AddSiteSupervisor = "WebPortal.UserManagement.AddSiteSupervisor";
				public const string AddSiteOperator = "WebPortal.UserManagement.AddSiteOperator";
				public const string AddSiteQA = "WebPortal.UserManagement.AddSiteQA";
				public const string AddAutomationStation = "WebPortal.UserManagement.AddAutomationStation";
				public const string AddFSCWebFinancialUser = "WebPortal.UserManagement.AddFSCWebFinancialUser";
				public const string AddClientWebFinancialUser = "WebPortal.UserManagement.AddClientWebFinancialUser";
				public const string AddTransportationUser = "WebPortal.UserManagment.AddTransportationUser";
				public const string AddCustomerServiceUser = "WebPortal.UserManagment.AddCustomerServiceUser";
				public const string CanAssignAllSites = "WebPortal.UserManagement.CanAssignAllSites";

				public static List<string> GetUserManagement()
				{
					return new List<string>
					{
						AddGeneralManager,
						AddSiteSupervisor,
						AddSiteOperator,
						AddSiteQA,
						AddAutomationStation,
						AddClientAdmin,
						AddClientUser,
						AddClientPackageSearchUser,
						AddSubClientAdmin,
						AddSubClientUser,
						AddAdministrator,
						AddFSCWebFinancialUser,
						AddClientWebFinancialUser,
						AddTransportationUser,
						AddCustomerServiceUser
					};
				}

				public static List<string> GetSiteUserManagement()
				{
					return new List<string> {
						AddGeneralManager,
						AddSiteSupervisor,
						AddSiteQA,
						AddSiteOperator,
						AddAutomationStation
					};
				}
			}

			public static class FileManagement
			{
				public const string EndOfDayFiles = "WebPortal.FileManagement.EndOfDayFiles";
				public const string AsnImports = "WebPortal.FileManagement.AsnImports";
				public const string AzureBlob = "WebPortal.FileManagement.AzureBlob";
				public const string ProgramManagementBlob = "WebPortal.FileManagement.ProgramManagementBlob";
			}

			public static class Dashboard
			{
				public const string AllSites = "WebPortal.Dashboard.AllSites";
				public const string SiteSpecific = "WebPortal.Dashboard.SiteSpecific";
				public static List<string> GetAllDashboardClaims()
				{
					return new List<string>
					{
					   AllSites,
					   SiteSpecific
					};
				}
			}

			public static class Reporting
			{
				public const string Admin = "WebPortal.Reporting.Admin";
				public const string ClientSpecific = "WebPortal.Reporting.ClientSpecific";
				public const string SubClientSpecific = "WebPortal.Reporting.SubClientSpecific";
				public const string SiteSpecific = "WebPortal.Reporting.SiteSpecific";

				public static List<string> GetAllReportingClaims()
				{
					return new List<string>
					{
					   Admin,
					   ClientSpecific,
					   SubClientSpecific,
					   SiteSpecific
					};
				}
			}

			public static class ServiceManagement
			{
				public const string ManageBinRules = "WebPortal.ServiceManagement.ManageBinRules";
				public const string ManageCostsAndCharges = "WebPortal.ServiceManagement.ManageCostsAndCharges";
				public const string ManageExtendedServiceRules = "WebPortal.ServiceManagement.ManageExtendedServiceRules";
				public const string ManageGeoDescriptors = "WebPortal.ServiceManagement.ManageGeoDescriptors";
				public const string ManageServiceRules = "WebPortal.ServiceManagement.ManageServiceRules";
				public const string ManageSiteAlerts = "WebPortal.ServiceManagement.ManageSiteAlerts";
				public const string ManageUspsRegions = "WebPortal.ServiceManagement.ManageUspsRegions";
				public const string ManageUspsHolidays = "WebPortal.ServiceManagement.ManageUspsHolidays";
				public const string ManageUspsEvsCodes = "WebPortal.ServiceManagement.ManageUspsEvsCodes";
				public const string ManageUspsVisnSite = "WebPortal.ServiceManagement.ManageUspsVisnSite";
				public const string ManageZipSchemas = "WebPortal.ServiceManagement.ManageZipSchemas";
				public const string ManageZoneMaps = "WebPortal.ServiceManagement.ManageZoneMaps";
				public const string ServiceOverride = "WebPortal.ServiceManagement.ServiceOverride";
				// Excludes ServiceOverride
				public static List<string> GetBaseServiceManagementClaims()
				{
					return new List<string> {
						ManageBinRules,
						ManageCostsAndCharges,
						ManageExtendedServiceRules,
						ManageGeoDescriptors,
						ManageServiceRules,
						ManageSiteAlerts,
						ManageUspsRegions,
						ManageUspsHolidays,
						ManageUspsEvsCodes,
						ManageUspsVisnSite,						
						ManageZipSchemas,
						ManageZoneMaps,
						ServiceOverride
					};
				}
			}

			public static class PackageManagement
			{
				public const string PackageSearch = "WebPortal.PackageManagement.PackageSearch";
				public const string PackageRecall = "WebPortal.PackageManagement.PackageRecall";
				public const string PackageRelease = "WebPortal.PackageManagement.PackageRelease";
				public const string ReadPackageRelease = "WebPortal.PackageManagement.ReadPackageRelease";
				public const string ReadPackageRecall = "WebPortal.PackageManagement.ReadPackageRecall";
				public const string DeleteRecallPackage = "WebPortal.PackageManagement.DeleteRecallPackage";
			}

			public static class ContainerManagement
			{
				public const string ContainerSearch = "WebPortal.ContainerManagement.ContainerSearch";
			}
		}

		public static class DesktopApplication
		{
			public static class Desktop
			{
				public const string ReceivingJobTicket = "DesktopApplication.Desktop.ReceivingJobTicket";
				public const string PackageLabel = "DesktopApplication.Desktop.PackageLabel";
				public const string ContainerLabel = "DesktopApplication.Desktop.ContainerLabel";
				public const string ContainerWeight = "DesktopApplication.Desktop.ContainerWeight";
				public const string PackageReprintReturns = "DesktopApplication.Desktop.PackageReprintReturns";
				public const string PackageHistory = "DesktopApplication.Desktop.PackageHistory";
			}
		}
		public static class Inquiry
		{
			public static class Helpdesk
			{
				public const string CreateTicket = "Inquiry.Helpdesk.CreateTicket";
				public const string RespondToTicket = "Inquiry.Helpdesk.RespondToTicket";
				public const string Reporting = "Inquiry.Helpdesk.Reporting";
			}
		}
		public static class Automation
		{
			public static class Numina
			{
				public const string SystemLogin = "Automation.Numina.SystemLogin";
			}
		}
	}
}
