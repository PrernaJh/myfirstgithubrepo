using PackageTracker.Data.Models;
using System.Collections.Generic;

namespace PackageTracker.Domain.Models.RecallRelease
{
	public class RecallReleasePackageResponse
	{
		public bool IsSuccessful { get; set; }
		public List<string> PackageIds { get; set; } = new List<string>();
		public List<string> FailedPackageIds { get; set; } = new List<string>();
		public List<string> LockedPackageIds { get; set; } = new List<string>();
		public List<Package> Packages { get; set; } = new List<Package>();
	}
}
