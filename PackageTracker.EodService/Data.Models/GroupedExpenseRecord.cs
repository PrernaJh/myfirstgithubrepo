using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PackageTracker.EodService.Data.Models
{
	public class GroupedExpenseRecord
	{
		public string SubClientName { get; set; }
		public string ProcessingDate { get; set; }
		public string BillingReference1 { get; set; }
		public string Product { get; set; }
		public string TrackingType { get; set; }
		public int Pieces { get; set; }
		public string SumWeight { get; set; }
		public string SumExtraServiceCost { get; set; }
		public string SumTotalCost { get; set; }
		public string AverageWeight { get; set; }
		public string AverageZone { get; set; }
	}
}
