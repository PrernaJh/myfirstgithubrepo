using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PackageTracker.Identity.Service.Models
{
	public class SignInRequest
	{
		[Required, MaxLength(50)]
		public string Username { get; set; }

		[Required, MaxLength(50)]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		public string RedirectUrl { get; set; }
	}

	public class SignInResponse
	{
		public bool Deactivated { get; }
		public bool ResetPassword { get; }
		public bool PasswordExpired { get; }
		public ClaimsPrincipal ClaimsPrincipal { get; }
		public string Token { get; }

		public SignInResponse(
			bool deactivated = false,
			bool resetPassword = false,
			bool passwordExpired = false,
			ClaimsPrincipal claimsPrincipal = null,
			string token = null)
		{
			Deactivated = deactivated;
			ResetPassword = resetPassword;
			PasswordExpired = passwordExpired;
			ClaimsPrincipal = claimsPrincipal;
			Token = token;
		}
	}
}
