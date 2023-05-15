using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
	public class ManageBinSchemasViewModel
	{
		[Required(ErrorMessage = "Start Date File is required.")]
		public string UploadStartDate { get; set; }
		[Required]
		public string SelectedSite { get; set; }
		public string SelectedClient { get; set; }
		[Required]
		public string SelectedSubClient { get; set; }
		[Required(ErrorMessage = "Bin Upload File is required.")]
		public IFormFile BinUploadFile { get; set; }
		[Required(ErrorMessage = "Five Digit Zip File is required.")]
		public IFormFile FiveDigitFile { get; set; }
		[Required(ErrorMessage = "Three Digit Zip File is required.")]
		public IFormFile ThreeDigitFile { get; set; }
	}
}
