namespace PackageTracker.Data.Models
{
	public class Client : Entity
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public bool IsEnabled { get; set; }
	}
}