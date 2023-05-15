using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
	public class UploadExtendedServiceRulesViewModel
	{
		public UploadExtendedServiceRulesViewModel()
		{
			UploadStartDate = DateTime.Now;
		}

		[Required(ErrorMessage = "Extended service rules are required.")]
		public IFormFile UploadFile { get; set; }
		[Required(ErrorMessage = "Start date is required.")]
		public DateTime UploadStartDate { get; set; }
		[Required(ErrorMessage = "SubClient is required.")]
		public string SubClientName { get; set; }
	}
}
