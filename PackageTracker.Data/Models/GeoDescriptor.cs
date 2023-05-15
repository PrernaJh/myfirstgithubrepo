namespace PackageTracker.Data.Models
{
	public class GeoDescriptor : Entity
	{
		public string ActiveGroupId { get; set; }
		public string Zip { get; set; }
		public string Description { get; set; }
		public string Value { get; set; }
	}
}
