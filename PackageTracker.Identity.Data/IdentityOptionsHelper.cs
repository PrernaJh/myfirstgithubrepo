using Microsoft.AspNetCore.Identity;

namespace PackageTracker.Identity.Data
{
	public static class IdentityOptionsHelper
	{
		public const int PasswordExpireDays = 90;

		public static IdentityOptions SetIdentityOptions(IdentityOptions options)
		{
			//options.Password.RequireDigit = true;
			//options.Password.RequireLowercase = true;
			//options.Password.RequireNonAlphanumeric = false;
			//options.Password.RequireUppercase = true;
			//options.Password.RequiredLength = 8;
			//options.Password.RequiredUniqueChars = 4;

			//// Lockout settings.
			//options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
			//options.Lockout.MaxFailedAccessAttempts = 5;
			//options.Lockout.AllowedForNewUsers = true;

			// User settings.
			options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
			options.User.RequireUniqueEmail = false;

			return options;
		}
	}
}
