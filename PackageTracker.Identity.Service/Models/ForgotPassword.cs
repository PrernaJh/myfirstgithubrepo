using System.ComponentModel.DataAnnotations;

namespace PackageTracker.Identity.Service.Models
{
	public class ForgotPasswordRequest
	{
		[Required]
		public string Username { get; set; }
	}

	public class ForgotPasswordResponse
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public string SecurityCode { get; set; }
	}
}
