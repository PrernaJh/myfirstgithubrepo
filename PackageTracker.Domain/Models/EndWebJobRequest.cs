using PackageTracker.Data.Models;
using System.Collections.Generic;

namespace PackageTracker.Domain.Models
{
	public class EndWebJobRequest
	{
		public WebJobRunRequest WebJobRun { get; set; }
		public bool IsSuccessful { get; set; }
		public int NumberOfRecords { get; set; }
		public string Message { get; set; }
		public List<FileDetail> FileDetails { get; set; } = new List<FileDetail>();
	}
}