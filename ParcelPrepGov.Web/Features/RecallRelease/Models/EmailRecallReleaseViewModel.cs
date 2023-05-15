using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;

namespace ParcelPrepGov.Web.Features.RecallRelease.Models
{
	public class EmailRecallReleaseViewModel
	{
		public EmailRecallReleaseViewModel(string host, string userName, DateTime siteLocaltime, 
			List<string> packageIds,  List<string> lockedPackageIds,  List<string> failedPackageIds,
			List<Package> packages, bool recallFlag = false)
		{
			Host = host;
			UserName = userName;
			RecallFlag = recallFlag;
			PackageIds.AddRange(packageIds);
			LockedPackageIds.AddRange(lockedPackageIds);
			FailedPackageIds.AddRange(failedPackageIds);
			Packages.AddRange(packages);
			Timestamp = siteLocaltime.ToString("g");
		}

		public string Host { get; private set; }
		public string UserName { get; private set; }
		public bool RecallFlag { get; private set; }
		public string Timestamp { get; set; }
		public string Email { get; set; }
		public List<string> PackageIds { get; private set; } = new List<string>();
		public List<string> FailedPackageIds { get; private set; } = new List<string>();
		public List<string> LockedPackageIds { get; private set; } = new List<string>();
		public List<Package> Packages { get; private set; } = new List<Package>();
		public string SiteGencoName { get; set; }
	}
}
