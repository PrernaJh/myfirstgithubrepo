using System.ComponentModel.DataAnnotations;

namespace PackageTracker.Domain.Models.Account
{
	public class UserModel
	{
		[Required]
		public string Username { get; set; }
		[Required]
		public string Password { get; set; }
	}
}
