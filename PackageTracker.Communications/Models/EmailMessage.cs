using System.Collections.Generic;

namespace PackageTracker.Communications.Models
{
	public class EmailMessage
	{
		public List<EmailAddress> ToAddresses { get; set; } = new List<EmailAddress>();
		public List<EmailAddress> FromAddresses { get; set; } = new List<EmailAddress>();
		public string Subject { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
	}
}
