using Microsoft.AspNetCore.Identity;
using PackageTracker.Identity.Data.Models;

namespace PackageTracker.Identity.Data
{
	public interface IDataInitializer
	{
		void SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager);
	}
}
