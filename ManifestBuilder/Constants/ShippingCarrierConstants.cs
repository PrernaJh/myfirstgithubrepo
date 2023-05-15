using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ManifestBuilder
{
	public static class ShippingCarrierConstants
	{
		public const string Usps = "USPS";
		public const string Ups = "UPS";
		public const string FedEx = "FEDEX";
		public const string Cmop = "CMOP";
		public const string Outside48States = "48STATES";
		public const string Missing = "MISSING";

		public static IEnumerable<string> ToList()
		{
			return typeof(ShippingCarrierConstants)
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
				.Select(x => (string)x.GetRawConstantValue())
				.ToList();
		}
	}
}
