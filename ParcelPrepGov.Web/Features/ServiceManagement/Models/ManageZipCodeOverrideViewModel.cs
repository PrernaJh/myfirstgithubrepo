using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
	public class ManageZipCodeOverrideViewModel
	{
		[Required(ErrorMessage = "Start Date File is required.")]
		public string UploadStartDate { get; set; }
		[Required(ErrorMessage = "SubClient is required.")]
		public string SelectedSubClient { get; set; }
		[Required(ErrorMessage = "Zipcode Override Upload File is required.")]
		public IFormFile UploadFile { get; set; }
	}
}
