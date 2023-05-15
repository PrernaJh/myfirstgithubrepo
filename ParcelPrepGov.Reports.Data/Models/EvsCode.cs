using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParcelPrepGov.Reports.Models
{
    public class EvsCode : UspsDataset
    {
		[StringLength(2)]
		public string Code { get; set; }
		[StringLength(50)]
		public string Description { get; set; }
		public int IsStopTheClock { get; set; }
		public int IsUndeliverable { get; set; }

	}
}
