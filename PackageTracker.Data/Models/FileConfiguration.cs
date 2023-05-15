namespace PackageTracker.Data.Models
{
	public class FileConfiguration : Entity
	{
		public string SiteName { get; set; }
		public string ConfigurationName { get; set; }
		public string FileDescription { get; set; }
		public string ScheduleType { get; set; }
		public string WebJobType { get; set; }
		public bool IsEnabled { get; set; }
	}
}
