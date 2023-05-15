using System.Collections.Generic;

namespace PackageTracker.Communications.Interfaces
{
	public interface IEmailConfiguration
	{
		string SmtpServer { get; }
		int SmtpPort { get; }
		string SmtpUsername { get; set; }
		string SmtpPassword { get; set; }

		List<string> ExceptionsEmailContactList { get; set; }
		List<string> ExceptionsSmsContactList { get; set; }
	}
}
