using System.Collections.Generic;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
	public class PostalPerformanceSummaryDetail
	{
		public PostalPerformance3Digit PostalPerformance3Digit { get; set; }
		public List<PostalPerformance5Digit> PostalPerformance5Digit { get; set; } = new List<PostalPerformance5Digit>();
	}
}