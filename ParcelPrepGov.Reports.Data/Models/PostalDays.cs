using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParcelPrepGov.Reports.Models
{
    public class PostalDays : UspsDataset
	{
		public DateTime PostalDate { get; set; } // Index
		public int Ordinal { get; set; } // Difference between Ordinal for two PostalDates is "Postal Days".  Sundays and holidays are skipped
		[StringLength(50)]
		public string Description { get; set; }
		public int IsHoliday { get; set; }
		public int IsSunday { get; set; }
	}
}
