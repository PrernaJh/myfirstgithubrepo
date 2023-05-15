using System.ComponentModel.DataAnnotations;

namespace PackageTracker.Domain.Models
{
    public class ModifyCritialEmailListRequest
	{
		public string SiteName { get; set; }

		public string Email { get; set; }
	}
}
