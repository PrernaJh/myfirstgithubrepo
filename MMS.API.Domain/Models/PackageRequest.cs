using System.ComponentModel.DataAnnotations;

namespace MMS.API.Domain.Models
{
	public class PackageRequest
	{
		[Required]
		//        [StringLength(32, MinimumLength = 32)] DALC package ID's are different.
		public string PackageId { get; set; }
		[Required]
		public string SiteName { get; set; }
		[Required]
		public string Username { get; set; }
		[Required]
		public string MachineId { get; set; }
	}
}
