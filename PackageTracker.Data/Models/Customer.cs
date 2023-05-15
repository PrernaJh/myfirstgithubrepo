namespace PackageTracker.Data.Models
{
	public class Customer : Entity
	{
		public string Name { get; set; }
		public string Key { get; set; }
		public string Type { get; set; }
		public string SiteName { get; set; }
		public string Description { get; set; }
		public string MailerId { get; set; }
		public bool IsEnabled { get; set; }
		public bool UseSimplifiedFileFormat { get; set; }
	}
}