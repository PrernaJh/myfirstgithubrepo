using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PackageTracker.Identity.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Identity.Service
{
	public class PPGPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : IdentityUser
	{
		private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
		private readonly IdentityOptions _identityOptions;

		public PPGPasswordValidator(IPasswordHasher<ApplicationUser> passwordHasher, IOptions<IdentityOptions> identityOptions = null)
		{
			_passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
			_identityOptions = (identityOptions ?? throw new ArgumentNullException(nameof(passwordHasher))).Value;
		}

		public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
		{
			//Must be 8 characters
			if (password.Trim().Length < 8)
				return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Passwords must be at least 8 characters.", Code = "MinLength8" }));

			//2 Upper case
			if (password.Count(char.IsUpper) < 2)
				return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Passwords must contain at least 2 uppercase letters.", Code = "MinUppercase2" }));

			//2 Lower case
			if (password.Count(char.IsLower) < 2)
				return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Passwords must contain at least 2 lowercase letters.", Code = "MinLowercase2" }));

			//4 numbers
			if (password.Count(char.IsNumber) < 4)
				return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Passwords must contain at least 4 numbers.", Code = "MinNumbers4" }));

			//No back to back characters or strings, for example, xx, 99, or 1234
			if (BackToBackCharactersExists(password))
				return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Passwords cannot contain consecutive equal characters.", Code = "ConsecutiveEqualChars" }));

			if (MoreThan2ConsecutiveSequentialNumbersExist(password))
				return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Passwords cannot contain more than 2 consecutive sequential numbers.", Code = "MoreThan2ConsecutiveSequentialNumbers" }));

			if (user != null && string.Equals(user.UserName, password, StringComparison.OrdinalIgnoreCase))
			{
				return Task.FromResult(IdentityResult.Failed(new IdentityError
				{
					Code = "UsernameAsPassword",
					Description = "You cannot use your username as your password"
				}));
			}

			// Existing users only
			// validate new and current passwords are not equal
			if (user != null && !string.IsNullOrEmpty(user.PasswordHash) &&
				_passwordHasher.VerifyHashedPassword(user as ApplicationUser, user.PasswordHash, password) == PasswordVerificationResult.Success)
			{
				return Task.FromResult(IdentityResult.Failed(new IdentityError
				{
					Code = "CurrentAndNewAreEqual",
					Description = "You cannot use your current password as your new password"
				}));
			}

			return Task.FromResult(IdentityResult.Success);
		}

		private static bool BackToBackCharactersExists(string password)
		{
			char[] passwordCharArray = password.ToLower().ToCharArray();

			for (int i = 1; i < passwordCharArray.Length; i++)
			{
				char currentChar = passwordCharArray[i];
				char prevChar = passwordCharArray[i - 1];

				if (currentChar.Equals(prevChar))
					return true;
			}

			return false;
		}

		private static bool MoreThan2ConsecutiveSequentialNumbersExist(string password)
		{
			char[] passwordCharArray = password.ToCharArray();

			for (int i = 2; i <= passwordCharArray.Length - 1; i++)
			{
				char firstChar = passwordCharArray[i - 2];
				char secondChar = passwordCharArray[i - 1];
				char thirdChar = passwordCharArray[i];

				if (!char.IsNumber(firstChar) || !char.IsNumber(secondChar))
					continue;

				if (char.GetNumericValue(firstChar) + 1 != char.GetNumericValue(secondChar))
					continue;

				if (char.GetNumericValue(secondChar) + 1 == char.GetNumericValue(thirdChar))
					return true;
			}

			return false;
		}

	}

}
