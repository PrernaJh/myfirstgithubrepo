using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PackageTracker.Domain.Utilities
{
	public static class WebJobSettingsUtility
	{
		public static string GetParameterStringValue(this WebJobSettings webJobSettings, string name, string defaultValue = "")
		{
			if (!webJobSettings.Parameters.TryGetValue(name, out string value))
				value = defaultValue;
			return value;
		}
		public static int GetParameterIntValue(this WebJobSettings webJobSettings, string name, int defaultValue = 0)
		{
			if (webJobSettings.Parameters.TryGetValue(name, out string stringValue) && int.TryParse(stringValue, out var value))
				return value;
			return defaultValue;
		}

		public static bool GetParameterBoolValue(this WebJobSettings webJobSettings, string name, bool defaultValue = false)
		{
			if (webJobSettings.Parameters.TryGetValue(name, out string stringValue) && bool.TryParse(stringValue, out var value))
				return value;
			return defaultValue;
		}

		public static bool IsDuringScheduledHours(this WebJobSettings webJobSettings, Site site, IEnumerable<SubClient> subClients)
        {
			return webJobSettings.IsDuringScheduledHours(new List<Site>() { site }, subClients);
        }

		public static bool IsDuringScheduledHours(this WebJobSettings webJobSettings, 
			IEnumerable<Site> sites, IEnumerable<SubClient> subClients = null)
		{
			if (subClients != null)
            {
				foreach (var subClient in subClients)
				{
					var site = sites.FirstOrDefault(s => s.SiteName == subClient.SiteName);
					if (site != null && webJobSettings.IsDuringScheduledHours(site, subClient))
						return true;
				}
            }
            else
            {
				foreach (var site in sites)
				{
					if (webJobSettings.IsDuringScheduledHours(site))
						return true;
				}
            }
			return false;
		}

		// Schedules for the webJob take precedence, then SubClient schedules, then Site schedules.
		// If there are no active schedules, then this returns true.
		// Note: Site must be passed in, even if not using the sites schedules, so that current time can be converted to local time.
		public static bool IsDuringScheduledHours(this WebJobSettings webJobSettings, Site site, SubClient subClient = null)
		{
			var schedules = webJobSettings.Schedules;
			if (schedules == null || schedules.Length == 0)
				schedules = subClient?.Schedules;
			if (schedules == null || schedules.Length == 0)
				schedules = site?.Schedules;
			if (schedules == null || schedules.Length == 0)
				return true;
			foreach (var schedule in schedules)
			{
				if (schedule.IsDuringScheduledHours(site.TimeZone))
					return true;
			}
			return false;
		}

		public static bool IsDuringScheduledHours(this Schedule schedule, string timeZone)
		{
			var now = DateTime.UtcNow; // Using UtcNow rather than Now, allows local debugging
			timeZone = TimeZoneUtility.ConvertTimeZoneConstantsToParameter(timeZone);
			if (StringHelper.Exists(timeZone))
				now = TimeZoneInfo.ConvertTime(now, TimeZoneInfo.FindSystemTimeZoneById(timeZone));
			var days = ParseDaysString(schedule.Days);
			if (!days.Contains(now.DayOfWeek))
				return false;
			var (start, end) = ParseHoursString(schedule.Hours);
			double hour = Hour(now);
			if (hour < start || hour >= end)
				return false;
			return true;
		}

		private static readonly IDictionary<string, DayOfWeek> dayAbreviations = new Dictionary<string, DayOfWeek>()
		{
			{ "Su", DayOfWeek.Sunday },
			{ "M", DayOfWeek.Monday },
			{ "Tu", DayOfWeek.Tuesday },
			{ "W", DayOfWeek.Wednesday },
			{ "Th", DayOfWeek.Thursday },
			{ "F", DayOfWeek.Friday },
			{ "Sa", DayOfWeek.Saturday }
		};

		private static IEnumerable<DayOfWeek> ParseDaysString(string daysString)
		{
			var days = new HashSet<DayOfWeek>();
			daysString = StringHelper.Exists(daysString) ? daysString : "Su-Sa";
			foreach (var part in daysString.Split(","))
			{
				if (StringHelper.Exists(part))
				{
					var parts = part.Split('-');
					if (parts.Length > 1)
					{
						if (dayAbreviations.TryGetValue(parts[0], out var first) &&
							dayAbreviations.TryGetValue(parts[1], out var last) &&
							(first <= last))
						{
							for (var day = first; day <= last; day++)
								days.Add(day);
						}
					}
					else
					{
						if (dayAbreviations.TryGetValue(part, out var day))
							days.Add(day);
					}
				}
			}
			return days;
		}

		private static double Hour(DateTime dateTime)
		{
			return dateTime.Hour + dateTime.Minute / 60.0 + dateTime.Second / 3600.0;
		}

		private static (double start, double end) ParseHoursString(string hoursString)
		{
			var start = 0.0;
			var end = 24.0;
			try
			{
				if (StringHelper.Exists(hoursString))
				{
					var parts = hoursString.Split("-");
					if (parts.Length > 1)
					{
						start = Hour(DateTime.Parse(parts[0]));
						end = Hour(DateTime.Parse(parts[1]));
					}
					else
					{
						start = Hour(DateTime.Parse(hoursString));
					}
				}
			}
			catch (Exception)
			{
			}
			return (start, end);
		}
	}
}
