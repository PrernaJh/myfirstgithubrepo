namespace PackageTracker.Domain.Models
{
	public class AssignPackageRatesResponse
	{
		public bool IsSuccessful { get; set; }
		public int NumberOfPackagesUpdated { get; set; }
		public int NumberOfPackagesFailed { get; set; }
	}
}
