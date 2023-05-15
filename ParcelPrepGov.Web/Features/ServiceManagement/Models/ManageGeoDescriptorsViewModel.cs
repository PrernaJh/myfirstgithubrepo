using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
    public class ManageGeoDescriptorsViewModel
    {
		[Required(ErrorMessage = "Start Date File is required.")]
		public string UploadStartDate { get; set; }
		[Required(ErrorMessage = "Site is required.")]
		public string SelectedSite { get; set; }
		[Required(ErrorMessage = "Upload File is required.")]
		public IFormFile UploadFile { get; set; }
	}
}
