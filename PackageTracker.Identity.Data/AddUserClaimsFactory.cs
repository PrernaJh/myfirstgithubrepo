using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PackageTracker.Identity.Data.Constants;
using PackageTracker.Identity.Data.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PackageTracker.Identity.Data
{
	public class AddUserClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
	{
		public AddUserClaimsFactory(
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			IOptions<IdentityOptions> optionsAccessor)
			: base(userManager, roleManager, optionsAccessor)
		{
		}

		protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
		{
			var identity = await base.GenerateClaimsAsync(user);
			var claims = new List<Claim>
			{
				new Claim(PPGClaim.Site, user.Site ?? string.Empty),
				new Claim(PPGClaim.Client, user.Client ?? string.Empty),
				new Claim(PPGClaim.SubClient, user.SubClient ?? string.Empty),
				new Claim(ClaimTypes.Email, user.Email?? string.Empty),
			};

			identity.AddClaims(claims);
			return identity;
		}
	}
}
