using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParcelPrepGov.Reports.Models
{
    public class PostalAreaAndDistrict : UspsDataset
    {
		[StringLength(3)]
		public string ZipCode3Zip { get; set; }
		public int Scf { get; set; }
		[StringLength(30)]
		public string PostalDistrict { get; set; }
		[StringLength(24)]
		public string PostalArea { get; set; }
	}
}
