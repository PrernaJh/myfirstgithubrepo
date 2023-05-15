using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParcelPrepGov.Reports.Models
{
	public class VisnSite : UspsDataset
	{
		[StringLength(12)]
		public string Visn { get; set; }
		[StringLength(12)]
		public string SiteParent { get; set; }
		[StringLength(12)]
		public string SiteNumber { get; set; }
		[StringLength(12)]
		public string SiteType { get; set; }
		[StringLength(60)]
		public string SiteName { get; set; }
		[StringLength(60)]
		public string SiteAddress1 { get; set; }
		[StringLength(60)]
		public string SiteAddress2 { get; set; }
		[StringLength(30)]
		public string SiteCity { get; set; }
		[StringLength(2)]
		public string SiteState { get; set; }
		[StringLength(10)]
		public string SiteZipCode { get; set; }
		[StringLength(24)]
		public string SitePhone { get; set; }
		[StringLength(30)]
		public string SiteShippingContact { get; set; }
	}
}
