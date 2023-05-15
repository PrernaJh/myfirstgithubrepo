using Microsoft.AspNetCore.Identity;
using PackageTracker.Identity.Data.Constants;
using PackageTracker.Identity.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace PackageTracker.Identity.Data
{
	/// <summary>
	/// This is an object that will help developers understand how users, roles, or claims work.
	/// If you have to add, update or remove a user, role, or claim _ 
	/// then object will help point you in the right direction.
	/// </summary>
	public static class DataInitializer
	{
		/// <summary>
		/// Seed data, init security for users, roles, and claims. 
		/// These methods setup the database on application run.
		/// DEV only if you are working with users, roles, or claims.
		/// </summary>
		/// <param name="userManager"></param>
		/// <param name="roleManager"></param>
		public static void SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			SeedRoles(roleManager);
			SeedRoleClaims(roleManager);
			SeedUsers(userManager);
		}

        private static void SeedRoleClaims(RoleManager<IdentityRole> roleManager)
        {
            if (roleManager.RoleExistsAsync(PPGRole.SystemAdministrator).Result)
            {
                var role = roleManager.FindByNameAsync(PPGRole.SystemAdministrator).Result;
                var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
				var requiredClaims = new List<string> {
					PPGClaim.WebPortal.UserManagement.AddSystemAdmin,
					PPGClaim.WebPortal.UserManagement.AddAdministrator,
					PPGClaim.WebPortal.UserManagement.AddClientAdmin,
					PPGClaim.WebPortal.UserManagement.AddClientUser,
					PPGClaim.WebPortal.UserManagement.AddClientPackageSearchUser,
					PPGClaim.WebPortal.UserManagement.AddSubClientAdmin,
					PPGClaim.WebPortal.UserManagement.AddSubClientUser,
					PPGClaim.WebPortal.UserManagement.AddGeneralManager,
					PPGClaim.WebPortal.UserManagement.AddSiteSupervisor,
					PPGClaim.WebPortal.UserManagement.AddSiteOperator,
					PPGClaim.WebPortal.UserManagement.AddSiteQA,
					PPGClaim.WebPortal.UserManagement.AddAutomationStation,
					PPGClaim.WebPortal.UserManagement.AddFSCWebFinancialUser,
					PPGClaim.WebPortal.UserManagement.AddClientWebFinancialUser,
					PPGClaim.WebPortal.UserManagement.AddTransportationUser,
					PPGClaim.WebPortal.UserManagement.AddCustomerServiceUser,
					PPGClaim.WebPortal.UserManagement.CanAssignAllSites,
					PPGClaim.WebPortal.ServiceManagement.ManageBinRules,
					PPGClaim.WebPortal.ServiceManagement.ManageServiceRules,
					PPGClaim.WebPortal.ServiceManagement.ManageExtendedServiceRules,
					PPGClaim.WebPortal.ServiceManagement.ManageGeoDescriptors,
					PPGClaim.WebPortal.ServiceManagement.ManageSiteAlerts,
                    PPGClaim.WebPortal.ServiceManagement.ManageZipSchemas,
                    PPGClaim.WebPortal.ServiceManagement.ManageZoneMaps,
                    PPGClaim.WebPortal.ServiceManagement.ManageCostsAndCharges,
                    PPGClaim.WebPortal.FileManagement.EndOfDayFiles,
                    PPGClaim.WebPortal.FileManagement.AsnImports,
					PPGClaim.WebPortal.FileManagement.AzureBlob,
					PPGClaim.WebPortal.FileManagement.ProgramManagementBlob,
					PPGClaim.WebPortal.Dashboard.AllSites,
                    PPGClaim.WebPortal.Reporting.Admin,
					PPGClaim.WebPortal.Reporting.SiteSpecific,
					PPGClaim.WebPortal.Reporting.ClientSpecific,
					PPGClaim.WebPortal.Reporting.SubClientSpecific,
					PPGClaim.WebPortal.PackageManagement.PackageSearch,
                    PPGClaim.WebPortal.PackageManagement.ReadPackageRecall,
                    PPGClaim.WebPortal.PackageManagement.ReadPackageRelease,
                    PPGClaim.WebPortal.PackageManagement.PackageRecall,
                    PPGClaim.WebPortal.PackageManagement.PackageRelease,
					PPGClaim.WebPortal.PackageManagement.DeleteRecallPackage,
                    PPGClaim.WebPortal.ServiceManagement.ServiceOverride,
                    PPGClaim.DesktopApplication.Desktop.ReceivingJobTicket,
                    PPGClaim.DesktopApplication.Desktop.PackageLabel,
                    PPGClaim.DesktopApplication.Desktop.ContainerLabel,
                    PPGClaim.DesktopApplication.Desktop.ContainerWeight,
                    PPGClaim.DesktopApplication.Desktop.PackageReprintReturns,
                    PPGClaim.DesktopApplication.Desktop.PackageHistory,
                    PPGClaim.Inquiry.Helpdesk.CreateTicket,
                    PPGClaim.Inquiry.Helpdesk.RespondToTicket,
                    PPGClaim.Inquiry.Helpdesk.Reporting

				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

			if (roleManager.RoleExistsAsync(PPGRole.Administrator).Result)
			{
				var role = roleManager.FindByNameAsync(PPGRole.Administrator).Result;
				var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
				var requiredClaims = new List<string> {
					PPGClaim.WebPortal.UserManagement.AddClientAdmin,
					PPGClaim.WebPortal.UserManagement.AddClientUser,
					PPGClaim.WebPortal.UserManagement.AddClientPackageSearchUser,
					PPGClaim.WebPortal.UserManagement.AddGeneralManager,
					PPGClaim.WebPortal.UserManagement.AddSiteSupervisor,
					PPGClaim.WebPortal.UserManagement.AddSiteOperator,
					PPGClaim.WebPortal.UserManagement.AddSiteQA,
					PPGClaim.WebPortal.UserManagement.AddAutomationStation,
					PPGClaim.WebPortal.UserManagement.AddTransportationUser,
					PPGClaim.WebPortal.UserManagement.AddCustomerServiceUser,
					PPGClaim.WebPortal.UserManagement.CanAssignAllSites,
					PPGClaim.WebPortal.ServiceManagement.ManageServiceRules,
					PPGClaim.WebPortal.ServiceManagement.ManageExtendedServiceRules,
					PPGClaim.WebPortal.ServiceManagement.ManageBinRules,
					PPGClaim.WebPortal.ServiceManagement.ManageCostsAndCharges,
					PPGClaim.WebPortal.ServiceManagement.ManageGeoDescriptors,
					PPGClaim.WebPortal.ServiceManagement.ManageSiteAlerts,
					PPGClaim.WebPortal.ServiceManagement.ManageZipSchemas,
					PPGClaim.WebPortal.ServiceManagement.ManageZoneMaps,
					PPGClaim.WebPortal.FileManagement.AsnImports,
					PPGClaim.WebPortal.Dashboard.AllSites,
					PPGClaim.WebPortal.Reporting.Admin,
					PPGClaim.WebPortal.FileManagement.AzureBlob,
					PPGClaim.WebPortal.FileManagement.ProgramManagementBlob,
					PPGClaim.WebPortal.PackageManagement.PackageSearch,
					PPGClaim.WebPortal.PackageManagement.ReadPackageRecall,
					PPGClaim.WebPortal.PackageManagement.ReadPackageRelease,
					PPGClaim.WebPortal.ServiceManagement.ServiceOverride,
					PPGClaim.DesktopApplication.Desktop.ReceivingJobTicket,
					PPGClaim.DesktopApplication.Desktop.PackageLabel,
					PPGClaim.DesktopApplication.Desktop.ContainerLabel,
					PPGClaim.DesktopApplication.Desktop.ContainerWeight,
					PPGClaim.DesktopApplication.Desktop.PackageReprintReturns,
					PPGClaim.DesktopApplication.Desktop.PackageHistory,
					PPGClaim.Inquiry.Helpdesk.CreateTicket,
					PPGClaim.Inquiry.Helpdesk.RespondToTicket,
					PPGClaim.Inquiry.Helpdesk.Reporting

				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

			if (roleManager.RoleExistsAsync(PPGRole.GeneralManager).Result)
			{
				var role = roleManager.FindByNameAsync(PPGRole.GeneralManager).Result;
				var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
				var requiredClaims = new List<string> {
					PPGClaim.WebPortal.UserManagement.AddSiteOperator,
					PPGClaim.WebPortal.UserManagement.AddSiteQA,
					PPGClaim.WebPortal.UserManagement.AddAutomationStation,
					PPGClaim.WebPortal.UserManagement.CanAssignAllSites,
					PPGClaim.WebPortal.FileManagement.EndOfDayFiles,
					PPGClaim.WebPortal.UserManagement.AddSiteSupervisor,
					PPGClaim.WebPortal.FileManagement.AsnImports,
					PPGClaim.WebPortal.Dashboard.SiteSpecific,
					PPGClaim.WebPortal.Reporting.SiteSpecific,
					PPGClaim.WebPortal.FileManagement.ProgramManagementBlob,
					PPGClaim.WebPortal.PackageManagement.PackageSearch,
					PPGClaim.WebPortal.PackageManagement.PackageRecall,
					PPGClaim.WebPortal.PackageManagement.PackageRelease,
					PPGClaim.WebPortal.PackageManagement.ReadPackageRecall,
					PPGClaim.WebPortal.PackageManagement.ReadPackageRelease,
					PPGClaim.WebPortal.PackageManagement.DeleteRecallPackage,
					PPGClaim.WebPortal.ServiceManagement.ServiceOverride,
					PPGClaim.DesktopApplication.Desktop.ReceivingJobTicket,
					PPGClaim.DesktopApplication.Desktop.PackageLabel,
					PPGClaim.DesktopApplication.Desktop.ContainerLabel,
					PPGClaim.DesktopApplication.Desktop.ContainerWeight,
					PPGClaim.DesktopApplication.Desktop.PackageReprintReturns,
					PPGClaim.DesktopApplication.Desktop.PackageHistory,
					PPGClaim.Inquiry.Helpdesk.CreateTicket,
				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

			if (roleManager.RoleExistsAsync(PPGRole.Supervisor).Result)
			{
				var role = roleManager.FindByNameAsync(PPGRole.Supervisor).Result;
				var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
				var requiredClaims = new List<string> {
					PPGClaim.WebPortal.UserManagement.AddSiteOperator,
					PPGClaim.WebPortal.UserManagement.AddSiteQA,
					PPGClaim.WebPortal.UserManagement.AddAutomationStation,
					PPGClaim.WebPortal.UserManagement.CanAssignAllSites,
					PPGClaim.WebPortal.FileManagement.EndOfDayFiles,
					PPGClaim.WebPortal.FileManagement.AsnImports,
					PPGClaim.WebPortal.Dashboard.SiteSpecific,
					PPGClaim.WebPortal.Reporting.SiteSpecific,
					PPGClaim.WebPortal.PackageManagement.PackageSearch,
					PPGClaim.WebPortal.PackageManagement.ReadPackageRecall,
					PPGClaim.WebPortal.PackageManagement.ReadPackageRelease,
					PPGClaim.WebPortal.ServiceManagement.ServiceOverride,
					PPGClaim.DesktopApplication.Desktop.ReceivingJobTicket,
					PPGClaim.DesktopApplication.Desktop.PackageLabel,
					PPGClaim.DesktopApplication.Desktop.ContainerLabel,
					PPGClaim.DesktopApplication.Desktop.ContainerWeight,
					PPGClaim.DesktopApplication.Desktop.PackageReprintReturns,
					PPGClaim.DesktopApplication.Desktop.PackageHistory,
					PPGClaim.Inquiry.Helpdesk.CreateTicket,

				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

			if (roleManager.RoleExistsAsync(PPGRole.Operator).Result)
			{
				var role = roleManager.FindByNameAsync(PPGRole.Operator).Result;
				var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
				var requiredClaims = new List<string> {
					PPGClaim.DesktopApplication.Desktop.PackageLabel,
					PPGClaim.DesktopApplication.Desktop.ContainerLabel,
					PPGClaim.DesktopApplication.Desktop.ContainerWeight,
					PPGClaim.DesktopApplication.Desktop.PackageHistory,

				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

			if (roleManager.RoleExistsAsync(PPGRole.QualityAssurance).Result)
			{
				var role = roleManager.FindByNameAsync(PPGRole.QualityAssurance).Result;
				var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
				var requiredClaims = new List<string> {
					PPGClaim.WebPortal.PackageManagement.PackageSearch,
					PPGClaim.WebPortal.PackageManagement.ReadPackageRecall,
					PPGClaim.WebPortal.PackageManagement.ReadPackageRelease,
					PPGClaim.DesktopApplication.Desktop.ReceivingJobTicket,
					PPGClaim.DesktopApplication.Desktop.PackageLabel,
					PPGClaim.DesktopApplication.Desktop.ContainerLabel,
					PPGClaim.DesktopApplication.Desktop.ContainerWeight,
					PPGClaim.DesktopApplication.Desktop.PackageReprintReturns,
					PPGClaim.DesktopApplication.Desktop.PackageHistory,
					PPGClaim.WebPortal.FileManagement.ProgramManagementBlob // for financials AND program management
				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

            if (roleManager.RoleExistsAsync(PPGRole.ClientWebAdministrator).Result)
            {
                var role = roleManager.FindByNameAsync(PPGRole.ClientWebAdministrator).Result;
                var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
                var requiredClaims = new List<string> {
                    PPGClaim.WebPortal.UserManagement.AddClientUser,
                    PPGClaim.WebPortal.UserManagement.AddSubClientAdmin,
                    PPGClaim.WebPortal.UserManagement.AddSubClientUser,
                    PPGClaim.WebPortal.Reporting.ClientSpecific,
                    PPGClaim.WebPortal.PackageManagement.PackageSearch,
                    PPGClaim.WebPortal.PackageManagement.ReadPackageRecall,
                    PPGClaim.WebPortal.PackageManagement.ReadPackageRelease,
                    PPGClaim.WebPortal.PackageManagement.PackageRecall,
                    PPGClaim.WebPortal.PackageManagement.PackageRelease,
					PPGClaim.WebPortal.FileManagement.ProgramManagementBlob
				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

            if (roleManager.RoleExistsAsync(PPGRole.ClientWebUser).Result)
            {
                var role = roleManager.FindByNameAsync(PPGRole.ClientWebUser).Result;
                var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
                var requiredClaims = new List<string> {
                    PPGClaim.WebPortal.Reporting.ClientSpecific,
                    PPGClaim.WebPortal.PackageManagement.PackageSearch,
                    PPGClaim.WebPortal.PackageManagement.ReadPackageRecall,
                    PPGClaim.WebPortal.PackageManagement.ReadPackageRelease,
                    PPGClaim.WebPortal.PackageManagement.PackageRecall,
                    PPGClaim.WebPortal.PackageManagement.PackageRelease,
					PPGClaim.WebPortal.FileManagement.ProgramManagementBlob,
					PPGClaim.WebPortal.FileManagement.AsnImports
				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

			if (roleManager.RoleExistsAsync(PPGRole.ClientWebPackageSearchUser).Result)
			{
				var role = roleManager.FindByNameAsync(PPGRole.ClientWebPackageSearchUser).Result;
				var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
				var requiredClaims = new List<string> {
					PPGClaim.WebPortal.PackageManagement.PackageSearch
				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

			if (roleManager.RoleExistsAsync(PPGRole.ClientWebFinancialUser).Result)
			{
				var role = roleManager.FindByNameAsync(PPGRole.ClientWebFinancialUser).Result;
				var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
				var requiredClaims = new List<string> {
					PPGClaim.WebPortal.FileManagement.AzureBlob,
					PPGClaim.WebPortal.FileManagement.AsnImports
				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

            if (roleManager.RoleExistsAsync(PPGRole.FSCWebFinancialUser).Result)
            {
				var role = roleManager.FindByNameAsync(PPGRole.FSCWebFinancialUser).Result;
				var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
				var requiredClaims = new List<string> {
					//PPGClaim.WebPortal.Policy.ServiceManagement 
					PPGClaim.WebPortal.ServiceManagement.ManageBinRules,
					PPGClaim.WebPortal.ServiceManagement.ManageCostsAndCharges, 
					PPGClaim.WebPortal.ServiceManagement.ManageExtendedServiceRules,
					PPGClaim.WebPortal.ServiceManagement.ManageGeoDescriptors,
					PPGClaim.WebPortal.ServiceManagement.ManageServiceRules,
					PPGClaim.WebPortal.ServiceManagement.ManageZipSchemas,
					PPGClaim.WebPortal.ServiceManagement.ManageZoneMaps,
					PPGClaim.WebPortal.FileManagement.AsnImports,
					PPGClaim.WebPortal.FileManagement.EndOfDayFiles,
					PPGClaim.WebPortal.FileManagement.AzureBlob
				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

			if (roleManager.RoleExistsAsync(PPGRole.TransportationUser).Result)
			{
				var role = roleManager.FindByNameAsync(PPGRole.TransportationUser).Result;
				var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
				var requiredClaims = new List<string> {
					PPGClaim.WebPortal.ServiceManagement.ManageBinRules,
					PPGClaim.WebPortal.Reporting.SiteSpecific
				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

			if (roleManager.RoleExistsAsync(PPGRole.SubClientWebAdministrator).Result)
            {
                var role = roleManager.FindByNameAsync(PPGRole.SubClientWebAdministrator).Result;
                var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
                var requiredClaims = new List<string> {
                    PPGClaim.WebPortal.UserManagement.AddSubClientUser,
                    PPGClaim.WebPortal.Reporting.SubClientSpecific,
                    PPGClaim.WebPortal.PackageManagement.PackageSearch,
                    PPGClaim.WebPortal.PackageManagement.ReadPackageRecall,
                    PPGClaim.WebPortal.PackageManagement.ReadPackageRelease,
                    PPGClaim.WebPortal.PackageManagement.PackageRecall,
                    PPGClaim.WebPortal.PackageManagement.PackageRelease

				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

            if (roleManager.RoleExistsAsync(PPGRole.SubClientWebUser).Result)
            {
                var role = roleManager.FindByNameAsync(PPGRole.SubClientWebUser).Result;
                var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
                var requiredClaims = new List<string> {
                    PPGClaim.WebPortal.Reporting.SubClientSpecific,
                    PPGClaim.WebPortal.PackageManagement.PackageSearch,
                    PPGClaim.WebPortal.PackageManagement.ReadPackageRecall,
                    PPGClaim.WebPortal.PackageManagement.ReadPackageRelease,
                    PPGClaim.WebPortal.PackageManagement.PackageRecall,
                    PPGClaim.WebPortal.PackageManagement.PackageRelease
                };

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

                foreach (var claim in missingClaims)
                {
                    roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
                }

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}

			if (roleManager.RoleExistsAsync(PPGRole.AutomationStation).Result)
			{
				var role = roleManager.FindByNameAsync(PPGRole.AutomationStation).Result;
				var currentClaims = roleManager.GetClaimsAsync(role).Result.Select(x => x.Type).ToList();
				var requiredClaims = new List<string> {
					PPGClaim.Automation.Numina.SystemLogin
				};

				var missingClaims = requiredClaims.Except(currentClaims).ToList();

				foreach (var claim in missingClaims)
				{
					roleManager.AddClaimAsync(role, new Claim(claim, "")).Wait();
				}

				var extraClaims = currentClaims.Except(requiredClaims).ToList();

				foreach (var claim in extraClaims)
				{
					roleManager.RemoveClaimAsync(role, new Claim(claim, "")).Wait();
				}
			}
		}

		private static void SeedRoles(RoleManager<IdentityRole> roleManager)
		{
			if (!roleManager.RoleExistsAsync(PPGRole.SystemAdministrator).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.SystemAdministrator;
				role.Name = PPGRole.SystemAdministrator;
				var roleResult = roleManager.CreateAsync(role).Result;
			}

			if (!roleManager.RoleExistsAsync(PPGRole.Administrator).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.Administrator;
				role.Name = PPGRole.Administrator;
				var roleResult = roleManager.CreateAsync(role).Result;
			}

			if (!roleManager.RoleExistsAsync(PPGRole.GeneralManager).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.GeneralManager;
				role.Name = PPGRole.GeneralManager;
				var roleResult = roleManager.CreateAsync(role).Result;
			}

			if (!roleManager.RoleExistsAsync(PPGRole.Supervisor).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.Supervisor;
				role.Name = PPGRole.Supervisor;
				var roleResult = roleManager.CreateAsync(role).Result;
			}

			if (!roleManager.RoleExistsAsync(PPGRole.Operator).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.Operator;
				role.Name = PPGRole.Operator;
				var roleResult = roleManager.CreateAsync(role).Result;
			}

			if (!roleManager.RoleExistsAsync(PPGRole.QualityAssurance).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.QualityAssurance;
				role.Name = PPGRole.QualityAssurance;
				var roleResult = roleManager.CreateAsync(role).Result;
			}

			if (!roleManager.RoleExistsAsync(PPGRole.ClientWebAdministrator).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.ClientWebAdministrator;
				role.Name = PPGRole.ClientWebAdministrator;
				var roleResult = roleManager.CreateAsync(role).Result;
			}

			if (!roleManager.RoleExistsAsync(PPGRole.ClientWebUser).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.ClientWebUser;
				role.Name = PPGRole.ClientWebUser;
				var roleResult = roleManager.CreateAsync(role).Result;
			}

			if (!roleManager.RoleExistsAsync(PPGRole.ClientWebPackageSearchUser).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.ClientWebPackageSearchUser;
				role.Name = PPGRole.ClientWebPackageSearchUser;
				var roleResult = roleManager.CreateAsync(role).Result;
			}

			if (!roleManager.RoleExistsAsync(PPGRole.ClientWebFinancialUser).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.ClientWebFinancialUser;
				role.Name = PPGRole.ClientWebFinancialUser;
				var roleResult = roleManager.CreateAsync(role).Result;
			}

			if (!roleManager.RoleExistsAsync(PPGRole.SubClientWebAdministrator).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.SubClientWebAdministrator;
				role.Name = PPGRole.SubClientWebAdministrator;
				var roleResult = roleManager.CreateAsync(role).Result;
			}

			if (!roleManager.RoleExistsAsync(PPGRole.SubClientWebUser).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.SubClientWebUser;
				role.Name = PPGRole.SubClientWebUser;
				var roleResult = roleManager.CreateAsync(role).Result;
			}

			if (!roleManager.RoleExistsAsync(PPGRole.AutomationStation).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.AutomationStation;
				role.Name = PPGRole.AutomationStation;
				var roleResult = roleManager.CreateAsync(role).Result;
			}

			if (!roleManager.RoleExistsAsync(PPGRole.FSCWebFinancialUser).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.FSCWebFinancialUser;
				role.Name = PPGRole.FSCWebFinancialUser;
				var roleResult = roleManager.CreateAsync(role).Result;
			}
			if (!roleManager.RoleExistsAsync(PPGRole.TransportationUser).Result)
			{
				var role = new IdentityRole();
				role.NormalizedName = PPGRole.TransportationUser;
				role.Name = PPGRole.TransportationUser;
				var roleResult = roleManager.CreateAsync(role).Result;
			} 
		}

		private static void SeedUsers(UserManager<ApplicationUser> userManager)
		{
			if (userManager.FindByNameAsync("tecadmin").Result == null)
			{
				var user = new ApplicationUser();
				user.UserName = "tecadmin";
				user.Email = "jghilardi@tecmailing.com";
				user.FirstName = "Tecmailing";
				user.LastName = "Administrator";
				user.Site = "GLOBAL";

				var result = userManager.CreateAsync(user, "PKGtracker4321!").Result;

				if (result.Succeeded)
				{
					userManager.AddToRoleAsync(user, PPGRole.SystemAdministrator).Wait();
				}
			}
		}
	}
}
