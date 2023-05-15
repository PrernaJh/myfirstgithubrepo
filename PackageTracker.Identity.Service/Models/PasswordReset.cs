using System.ComponentModel.DataAnnotations;

namespace PackageTracker.Identity.Service.Models
{
	public class PasswordResetRequest
	{
		public string Username { get; set; }

		public string CurrentPassword { get; set; }

		[Required(ErrorMessage = "New password is required")]
		public string NewPassword { get; set; }

		[Required(ErrorMessage = "Confirm Password is required")]
		[Compare("NewPassword", ErrorMessage = "'New Password' and 'Confirm Password' do not match.")]
		public string ConfirmPassword { get; set; }

		public string RedirectUrl { get; set; }
	}

	public class PasswordResetResponse
	{
		public string RedirectUrl { get; }

		public PasswordResetResponse(string redirectUrl = null)
		{
			RedirectUrl = redirectUrl;
		}
	}
}
