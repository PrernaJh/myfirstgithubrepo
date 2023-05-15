using Microsoft.Extensions.Options;

namespace PackageTracker.Communications.SMS
{
	public class SmsConfiguration
	{
		public const string TwilioSmsSection = "TwilioSMS";

		public string AccountSid { get; set; }
		public string AuthToken { get; set; }
		public string PhoneNumber { get; set; }
	}

	public class SmsConfigurationValidation : IValidateOptions<SmsConfiguration>
	{
		public ValidateOptionsResult Validate(string name, SmsConfiguration options)
		{
			if (string.IsNullOrEmpty(options.AccountSid))
			{
				return ValidateOptionsResult.Fail($"{nameof(SmsConfiguration)} appsettings validation failure: {nameof(options.AccountSid)}");
			}

			if (string.IsNullOrEmpty(options.AuthToken))
			{
				return ValidateOptionsResult.Fail($"{nameof(SmsConfiguration)} appsettings validation failure: {nameof(options.AuthToken)}");
			}

			if (string.IsNullOrEmpty(options.PhoneNumber))
			{
				return ValidateOptionsResult.Fail($"{nameof(SmsConfiguration)} appsettings validation failure: {nameof(options.PhoneNumber)}");
			}

			return ValidateOptionsResult.Success;
		}
	}

}
