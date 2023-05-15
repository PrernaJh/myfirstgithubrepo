using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class RecallReleaseSummary
    {
		[Display(Name ="Recall Status")]
		public string PackageStatus { get; set; }
		public int Num_Packages { get; set; }
		public string Status { get; set; }
	}
}
