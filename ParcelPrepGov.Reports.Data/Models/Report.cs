using System;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Reports.Models
{
    public class Report
    {
		[Key]
		[StringLength(256)]
		public string ReportName { get; set; }
		[StringLength(32)]
		[Required]
		public string Site { get; set; }
		[Required]
		[StringLength(32)]
		public string Client { get; set; }
		[Required]
		[StringLength(32)]
		public string SubClient { get; set; }
		[Required]
		[StringLength(512)]
		public string Role { get; set; }
		[StringLength(256)]
		public string UserName { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime ChangeDate { get; set; }
		public string ReportXml { get; set; }
		public bool IsGlobal { get; set; }
		public bool IsReadOnly { get; set; }
	}
}
