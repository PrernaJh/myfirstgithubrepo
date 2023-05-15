using PackageTracker.Communications.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace PackageTracker.Communications.Interfaces
{
	public interface IEmailService
	{
		void Send(EmailMessage emailMessage);
		Task SendAsync(EmailMessage emailMessage, bool isHtml = false, IList<EmailAttachment> attachments = null);
		void SendServiceErrorNotifications(string jobName, string message);
		void SendServiceAlertNotifications(string subject, string message,
			List<string> criticalAlertEmailGroups, List<string> criticalAlertSmsContactGroups);
	}
}
