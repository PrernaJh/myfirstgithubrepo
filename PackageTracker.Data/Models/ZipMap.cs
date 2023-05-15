namespace PackageTracker.Data.Models
{
	public class ZipMap : Entity
	{
		public string ZipCode { get; set; }
		public string ActiveGroupType { get; set; }
		public string ActiveGroupId { get; set; }
		public string Value { get; set; }
	}
}
