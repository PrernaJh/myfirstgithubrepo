using System.ComponentModel.DataAnnotations;

namespace MMS.API.Domain.Models
{
	public class ScanPackageRequest : PackageRequest
	{
		public string Weight { get; set; }
		[Required]
		public string JobId { get; set; }
	}
}
