using System;

namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
	public class ActiveGroupViewModel
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Filename { get; set; }
		public DateTime StartDate { get; set; }
		public string UploadedBy { get; set; }
		public DateTime UploadDate { get; set; }
	}
}
