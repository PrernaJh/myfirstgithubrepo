using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
    public class UploadBusinessRulesViewModel
	{
		public UploadBusinessRulesViewModel()
		{
			UploadStartDate = DateTime.Now;
		}

		[Required(ErrorMessage = "Business rules are required.")]
		public IFormFile UploadFile { get; set; }
		[Required(ErrorMessage = "Start date is required.")]
		public DateTime UploadStartDate { get; set; }
		[Required(ErrorMessage = "Customer is required.")]
		public string CustomerName { get; set; }
	}
}
