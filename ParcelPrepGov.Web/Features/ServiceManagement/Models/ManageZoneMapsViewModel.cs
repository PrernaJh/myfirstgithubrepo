using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
	public class ManageZoneMapsViewModel
	{
		[Required]
		public IFormFile UploadFile { get; set; }
		[Required]
		public string StartDate { get; set; }
	}
}
