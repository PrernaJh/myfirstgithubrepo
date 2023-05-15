using PackageTracker.Communications.Interfaces;
using System.Collections.Generic;

namespace PackageTracker.Communications
{
	public class EmailConfiguration : IEmailConfiguration
	{
		public EmailConfiguration()
		{
			ExceptionsEmailContactList = new List<string>();
			ExceptionsSmsContactList = new List<string>();
		}

		public string SmtpServer { get; set; }
		public int SmtpPort { get; set; }
		public string SmtpUsername { get; set; }
		public string SmtpPassword { get; set; }

		public List<string> ExceptionsEmailContactList { get; set; }
		public List<string> ExceptionsSmsContactList { get; set; }
	}
}
