using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using PackageTracker.Identity.Data.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Infrastructure.Extensions
{
	public static class PrincipalExtensions
	{
		public static string GetClaimValue(this IPrincipal currentPrincipal, string key)
		{
			if (!currentPrincipal.Identity.IsAuthenticated || !(currentPrincipal.Identity is ClaimsIdentity identity))
				return null;

			var claim = identity.Claims.FirstOrDefault(c => c.Type == key);

			return claim?.Value;
		}

		public static bool HasClaimType(this IPrincipal currentPrincipal, string key)
		{
			return currentPrincipal.GetClaimValue(key) != null;
		}

		public static string GetSite(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated || !(currentPrincipal.Identity is ClaimsIdentity identity))
				return null;

			var claim = identity.Claims.FirstOrDefault(c => c.Type == PPGClaim.Site);

			return claim?.Value;
		}

		public static string GetClient(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated || !(currentPrincipal.Identity is ClaimsIdentity identity))
				return null;

			var claim = identity.Claims.FirstOrDefault(c => c.Type == PPGClaim.Client);

			return claim?.Value;
		}

		public static string GetSubClient(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated || !(currentPrincipal.Identity is ClaimsIdentity identity))
				return null;

			var claim = identity.Claims.FirstOrDefault(c => c.Type == PPGClaim.SubClient);

			return claim?.Value;
		}

        public static List<string> GetRoles(this IPrincipal currentPrincipal)
        {
            if (!currentPrincipal.Identity.IsAuthenticated || !(currentPrincipal.Identity is ClaimsIdentity identity))
                return null;

            var roles = identity.Claims
                       .Where(c => c.Type == ClaimTypes.Role)
                       .Select(c => c.Value)
                       .ToList();

            return roles;
        }

        public static string GetUsername(this IPrincipal currentPrincipal)
        {
            if (!currentPrincipal.Identity.IsAuthenticated || !(currentPrincipal.Identity is ClaimsIdentity identity))
                return default;

			return identity.FindFirst(ClaimTypes.Name).Value;
		}

		public static bool IsSystemAdministrator(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.SystemAdministrator);
		}
		public static bool IsGeneralManager(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.GeneralManager);
		}

		public static bool IsAdministrator(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.Administrator);
		}

		public static bool IsUserManager(this ClaimsPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.HasClaim(c => (PPGClaim.WebPortal.UserManagement.GetUserManagement().Contains(c.Type)));
		}

		public static bool IsSingleUserManager(this ClaimsPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			List<bool> userManagers = new List<bool>
			{
				currentPrincipal.IsSystemAdministrator(),
				currentPrincipal.HasClaim(c => (PPGClaim.WebPortal.UserManagement.GetSiteUserManagement().Contains(c.Type))),
				currentPrincipal.HasClaim(c => c.Type == PPGClaim.WebPortal.UserManagement.AddClientUser || c.Type == PPGClaim.WebPortal.UserManagement.AddClientAdmin),
				currentPrincipal.HasClaim(c => c.Type == PPGClaim.WebPortal.UserManagement.AddSubClientUser || c.Type == PPGClaim.WebPortal.UserManagement.AddSubClientAdmin)
			};

			return userManagers.Count(x => x) == 1;
		}

		public static bool IsSupervisor(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.Supervisor);
		}

		public static bool IsQualityAssurance(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.QualityAssurance);
		}
		public static bool IsSiteOperator(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.Operator);
		}

		public static bool IsClientWebAdministrator(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.ClientWebAdministrator);
		}

		public static bool IsClientWebFinancialUser(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.ClientWebFinancialUser);
		}

		public static bool IsCustomerService(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.CustomerService);
		}

		public static bool IsFscWebFinancialUser(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.FSCWebFinancialUser);
		}
		public static bool IsTransportationUser(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.TransportationUser);
		}

		public static bool IsClientWebUser(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.ClientWebUser);
		}

		public static bool IsClientWebPackageSearchUser(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.ClientWebPackageSearchUser);
		}

		public static bool IsSubClientWebAdministrator(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.SubClientWebAdministrator);
		}

		public static bool IsSubClientWebUser(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated)
				return false;

			return currentPrincipal.IsInRole(PPGRole.SubClientWebUser);
		}

		public static string GetEmail(this IPrincipal currentPrincipal)
		{
			if (!currentPrincipal.Identity.IsAuthenticated || !(currentPrincipal.Identity is ClaimsIdentity identity))
				return default;

			return identity.FindFirst(ClaimTypes.Email).Value;
		}

	}
}
