using System.Collections.Generic;

namespace PackageTracker.Domain.Models.ActiveGroupDetails
{
	public class GetBinDetailsResponse
	{
		public List<BinDetails> BinDetails { get; set; } = new List<BinDetails>();
	}

	public class BinDetails
	{
		public string Name { get; set; }
		public string ActiveGroupId { get; set; }
		public string StartDate { get; set; }
		public string CreateDate { get; set; }
		public string AddedBy { get; set; }
	}
}
