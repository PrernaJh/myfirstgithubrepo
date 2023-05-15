using System.Collections.Generic;

namespace PackageTracker.Domain.Models.ActiveGroupDetails
{
	public class GetServiceRuleDetailsResponse
	{
		public List<ServiceRuleDetails> ServiceRuleDetails { get; set; } = new List<ServiceRuleDetails>();
	}

	public class ServiceRuleDetails
	{
		public string SubClientName { get; set; }
		public string ActiveGroupId { get; set; }
		public string StartDate { get; set; }
		public string CreateDate { get; set; }
		public string AddedBy { get; set; }
	}
}
