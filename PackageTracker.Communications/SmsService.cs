using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Communications.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace PackageTracker.Communications.SMS
{
	public class SmsService : ISmsService
	{
		private SmsConfiguration smsConfiguration;

		public SmsService(ILogger<SmsService> logger, IOptionsMonitor<SmsConfiguration> options)
		{

			smsConfiguration = options.CurrentValue;

			options.OnChange(config =>
			{
				smsConfiguration = config;
				logger.Log(LogLevel.Information, $"{nameof(SmsConfiguration)} updated.");
			});

		}

		public async Task<SmsResponse> SendAsync(SmsMessage smsMessage)
		{
			var result = new List<ValidationResult>();
			var context = new ValidationContext(smsMessage, null, null);
			var isValid = Validator.TryValidateObject(smsMessage, context, result, true);

			if (!isValid)
			{
				throw new ArgumentException(string.Join(" ", result.Select(x => x.ErrorMessage)));
			}

			TwilioClient.Init(smsConfiguration.AccountSid, smsConfiguration.AuthToken);

			var message = await MessageResource.CreateAsync(
				from: new Twilio.Types.PhoneNumber(smsConfiguration.PhoneNumber),
				body: smsMessage.Body,
				to: new Twilio.Types.PhoneNumber(smsMessage.ToPhoneNumber)
			);

			return new SmsResponse(message.Status.ToString(), message.ErrorCode, message.ErrorMessage);

		}
	}
}
