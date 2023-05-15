using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Data.Utilities
{
	public static class WeightUtility
	{
		public static decimal ConvertOuncesToPounds(decimal weightInOunces)
		{
			return weightInOunces / 16;
		}

		public static decimal GenerateBillingWeight(decimal weightInOunces)
		{
			//ceiling in ounces
			return Math.Ceiling(weightInOunces);
		}
	}
}
