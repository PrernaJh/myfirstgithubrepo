namespace PackageTracker.Data.Models.JobOptions
{
	public class PackageDescription : ValueOption
	{
		public string PackageType { get; set; }
		public string Length { get; set; }
		public string Width { get; set; }
		public string Depth { get; set; }
	}
}
