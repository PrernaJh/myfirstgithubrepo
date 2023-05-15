using Microsoft.Extensions.Options;

namespace MMS.API.Domain.ZplUtilities
{
	public class ZPLConfigurationValidation : IValidateOptions<ZPLConfiguration>
	{
		public ValidateOptionsResult Validate(string name, ZPLConfiguration options)
		{
			//if (string.IsNullOrEmpty(options.NuminaLabelTemplate))
			//{
			//    return ValidateOptionsResult.Fail($"{nameof(ZPLConfiguration)} validation failure: No Numina label template found in the appsettings file.");
			//}

			//if (string.IsNullOrEmpty(options.NuminaErrorLabelTemplate))
			//{
			//    return ValidateOptionsResult.Fail($"{nameof(ZPLConfiguration)} validation failure: No Numina error label template found in the appsettings file.");
			//}

			return ValidateOptionsResult.Success;
		}
	}
}