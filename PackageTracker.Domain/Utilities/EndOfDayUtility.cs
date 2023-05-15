using PackageTracker.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Domain.Utilities
{
	public static class EndOfDayUtility
	{
		public static (int LookbackStart, int LookbackEnd) GetDefaultEndOfDayLookbacks()
		{
			return (7, -1);
		}

		public static (int LookbackStart, int LookbackEnd) GetLookbacksFromTargetDate(DateTime targetDate, string timeZone)
		{
			var localCurrentDate = TimeZoneUtility.GetLocalTime(timeZone).Date;

			var lookbackStart = (int)(localCurrentDate.Date - targetDate.Date).TotalDays;
			var lookbackEnd = lookbackStart - 1;

			return (lookbackStart, lookbackEnd);
		}
	}
}
