using MailKit.Net.Smtp;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using MimeKit;
using MimeKit.Text;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Communications.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Communications
{
	public class EmailService : IEmailService
	{
		private readonly IEmailConfiguration emailConfiguration;

		public EmailService(IEmailConfiguration emailConfiguration)
		{
			this.emailConfiguration = emailConfiguration;
		}

		public void Send(EmailMessage emailMessage)
		{
			var message = GenerateMessage(emailMessage, TextFormat.Plain);

			using (var emailClient = new SmtpClient()) // be sure to use MailKit.Net.Smtp, not System.Net
			{
				emailClient.Connect(emailConfiguration.SmtpServer, emailConfiguration.SmtpPort);
				emailClient.Authenticate(emailConfiguration.SmtpUsername, emailConfiguration.SmtpPassword);
				emailClient.Send(message);
				emailClient.Disconnect(true);
			}
		}

		public async Task SendAsync(EmailMessage emailMessage, bool isHtml = false,
			IList<EmailAttachment> attachments = null)
		{
			var textFormat = isHtml ? TextFormat.Html : TextFormat.Plain;
			var message = GenerateMessage(emailMessage, textFormat, attachments);

			using (var emailClient = new SmtpClient()) // be sure to use MailKit.Net.Smtp, not System.Net
			{
				await emailClient.ConnectAsync(emailConfiguration.SmtpServer, emailConfiguration.SmtpPort);
				await emailClient.AuthenticateAsync(emailConfiguration.SmtpUsername, emailConfiguration.SmtpPassword);
				await emailClient.SendAsync(message);
				await emailClient.DisconnectAsync(true);
			}
		}

		private static MimeMessage GenerateMessage(EmailMessage emailMessage, TextFormat textFormat, 
			IList<EmailAttachment> attachments = null)
		{
			var message = new MimeMessage();
			message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
			message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
			message.Subject = emailMessage.Subject;
			message.Body = new TextPart(textFormat) // html option
			{
				Text = emailMessage.Content
			};
			if (attachments != null)
            {
				var multipart = new Multipart("mixed");
				multipart.Add(message.Body);
				foreach (var attachment in attachments)
                {
					var parts = attachment.MimeType.Split("/", 2);
					var mediaType = parts[0];
					var mediaSubtype = parts.Length > 1 ? parts[1] : string.Empty;
					var element = new MimePart(mediaType, mediaSubtype)
					{
						Content = new MimeContent(new MemoryStream(attachment.FileContents), ContentEncoding.Default),
						ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
						ContentTransferEncoding = ContentEncoding.Base64,
						FileName = attachment.FileName
					};
					multipart.Add(element);
				}
				message.Body = multipart;
			}
			return message;
		}

		public void SendServiceErrorNotifications(string jobName, string message)
		{
			// send email
			var email = new EmailMessage();
			emailConfiguration.ExceptionsEmailContactList.Where(x => !StringUtilities.IsNullOrWhiteSpace(x)).ToList()
				.ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
			if (email.ToAddresses.Any())
			{
				email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
				email.Subject = $"{jobName}: Exception";
				email.Content = $"{message}";
				Send(email);
			}

			// send sms
			var sms = new EmailMessage();
			emailConfiguration.ExceptionsSmsContactList.Where(x => !StringUtilities.IsNullOrWhiteSpace(x)).ToList()
				.ForEach(x => sms.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
			if (sms.ToAddresses.Any())
			{
				sms.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
				sms.Content = $"{jobName}: Exception during processing.  See email or logs for details";
				Send(sms);
			}
		}
		public void SendServiceAlertNotifications(string subject, string message,
			List<string> criticalAlertEmailGroups, List<string> criticalAlertSmsContactGroups)
		{
			// send email
			var email = new EmailMessage();
			criticalAlertEmailGroups.Where(x => !StringUtilities.IsNullOrWhiteSpace(x)).ToList()
				.ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
			if (email.ToAddresses.Any())
			{
				email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
				email.Subject = subject;
				email.Content = message;
				Send(email);
			}
		}
	}
}