using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
	public class ManageZipSchemasViewModel
	{
		[Required]
		public IFormFile FedExHawaiiUploadFile { get; set; }
		[Required]
		public IFormFile UpsNdaSat48File { get; set; }
		[Required]
		public IFormFile UpsDasFile { get; set; }
		[Required]
		public IFormFile UspsRuralFile { get; set; }
		[Required]
		public string FedExHawaiiStartDate { get; set; }
		[Required]
		public string UpsNdaSat48StartDate { get; set; }
		[Required]
		public string UpsDasStartDate { get; set; }
		[Required]
		public string UspsRuralStartDate { get; set; }
	}
}
