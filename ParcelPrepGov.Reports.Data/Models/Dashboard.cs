using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Reports.Models
{
	public class Dashboard
	{
		public int Id { get; set; }
		[StringLength(24)]
		[Required]
		public string Site { get; set; }
		[StringLength(256)]
		public string UserName { get; set; }
		public string DashboardXml { get; set; }
		[StringLength(256)]
		public string DashboardName { get; set; }
		public bool IsGlobal { get; set; }
	}
}
