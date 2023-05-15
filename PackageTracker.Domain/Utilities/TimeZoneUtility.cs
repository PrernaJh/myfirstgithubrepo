using System;

namespace PackageTracker.Data.Utilities
{
	public static class TimeZoneUtility
	{
		public const string EasternTimeZone = "EASTERN";
		public const string CentralTimeZone = "CENTRAL";
		public const string MountainTimeZone = "MOUNTAIN";
		public const string MountainStandardTimeZone = "MOUNTAIN_STANDARD";
		public const string PacificTimeZone = "PACIFIC";

		public static DateTime GetLocalTime(string timeZone)
		{
			return TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(ConvertTimeZoneConstantsToParameter(timeZone)));
		}

		public static string ConvertTimeZoneConstantsToParameter(string input)
		{
			if (input == EasternTimeZone)
			{
				return "Eastern Standard Time";
			}
			else if (input == CentralTimeZone)
			{
				return "Central Standard Time";
			}
			else if (input == MountainTimeZone)
			{
				return "Mountain Standard Time";
			}
			else if (input == MountainStandardTimeZone)
			{
				return "US Mountain Standard Time";
			}
			else if (input == PacificTimeZone)
			{
				return "Pacific Standard Time";
			}
			else
			{
				return input;
			}
		}
	}
}