using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ManifestBuilder
{
	public static class ServiceTypeConstants
	{
		// usps
		public const string UspsFirstClass = "FIRST_CLASS";
		public const string UspsPriority = "PRIORITY_MAIL";
		public const string UspsParcelSelectLightWeight = "PSLW";
		public const string UspsParcelSelect = "PS";
		public const string UspsPriorityExpress = "EXPRESS_MAIL";
		public const string UspsPmod = "PMOD";

		public const string UspsPmodBag = "PMOD_BAG";
		public const string UspsPmodPallet = "PMOD_PALLET";

		// ups
		public const string UpsGround = "GROUND";
		public const string UpsNextDayAir = "NEXT_DAY_AIR";
		public const string UpsNextDayAirSaver = "NEXT_DAY_AIR_SAVER";
		public const string UpsSecondDayAir = "SECOND_DAY_AIR";

		// fedex
		public const string FedExPriorityOvernight = "PRIORITY_OVERNIGHT";
		public const string FedExGround = "GROUND";

		// domain specific
		public const string Outside48States = "48STATES";
		public const string ReturnToCustomer = "RETURN";
		public const string Missing = "MISSING";

		public static IEnumerable<string> ToList()
		{
			return typeof(ServiceTypeConstants)
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
				.Select(x => (string)x.GetRawConstantValue())
				.ToList();
		}
	}
}
