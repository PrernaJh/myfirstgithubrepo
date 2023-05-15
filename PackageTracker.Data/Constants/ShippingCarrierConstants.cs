using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace PackageTracker.Data.Constants
{
	public static class ShippingCarrierConstants
	{
		public const string Usps = "USPS";
		public const string Ups = "UPS";
		public const string FedEx = "FEDEX";
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

		public static readonly IEnumerable<string> ValidSinglePackageCarriers = new ReadOnlyCollection<string>(new List<string>
		{
			Usps
		});
	}
}
