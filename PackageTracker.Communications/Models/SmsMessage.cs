using System.ComponentModel.DataAnnotations;

namespace PackageTracker.Communications.Models
{
	public class SmsMessage
	{
		[Required]
		[MaxLength(200)]
		public string Body { get; }

		[Required]
		[Phone]
		[RegularExpression(@"^\(?([0-9]{3})\)?[-.●]?([0-9]{3})[-.●]?([0-9]{4})$", ErrorMessage = "The To field is not a valid phone number.")]
		public string ToPhoneNumber { get; }

		public SmsMessage(string body, string toPhoneNumber)
		{
			Body = body;
			ToPhoneNumber = toPhoneNumber;
		}
	}
}
