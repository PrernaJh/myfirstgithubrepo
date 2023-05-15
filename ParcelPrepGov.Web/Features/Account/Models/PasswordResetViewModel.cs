using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.Account.Models
{
	public class PasswordResetViewModel
	{
		[Required]
		[MaxLength(200)]
		[DataType(DataType.Password)]
		[Display(Name = "Your new password")]
		public string NewPassword { get; set; }

		[Required]
		[MaxLength(200)]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm your new password")]
		[Compare("NewPassword")]
		public string ConfirmPassword { get; set; }

		public string SecurityCode { get; set; }
	}
}
