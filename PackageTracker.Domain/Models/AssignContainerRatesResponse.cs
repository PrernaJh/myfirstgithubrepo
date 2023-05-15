namespace PackageTracker.Domain.Models
{
	public class AssignContainerRatesResponse
	{
		public bool IsSuccessful { get; set; }
		public int NumberOfContainersUpdated { get; set; }
		public int NumberOfContainersFailed { get; set; }
	}
}
