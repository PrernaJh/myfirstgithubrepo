using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Reports.Models
{
	public class SubClientDataset : Dataset
	{
		[StringLength(50)]
		public string Name { get; set; }
		[StringLength(50)]
		public string Description { get; set; }
		[StringLength(50)]
		public string Key { get; set; }
		[StringLength(50)]
		public string ClientName { get; set; }
	}
}
