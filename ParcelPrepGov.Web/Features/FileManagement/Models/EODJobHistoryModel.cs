using System;

namespace ParcelPrepGov.Web.Features.FileManagement.Models
{
	public class EODJobHistoryModel
	{
		public string Username { get; set; }
		public string JobName { get; set; }
		public DateTime CreateDate { get; set; }
		public string FileName { get; set; }
		public bool IsSuccessful { get; set; }
		public string ErrorMessage { get; set; }
	}
}
