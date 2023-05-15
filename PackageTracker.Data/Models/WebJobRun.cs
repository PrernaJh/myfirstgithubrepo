using System;
using System.Collections.Generic;

namespace PackageTracker.Data.Models
{
	public class WebJobRun : Entity
	{
		public string SiteName { get; set; }
		public string ClientName { get; set; }
		public string SubClientName { get; set; }
		public string JobName { get; set; }
		public string JobType { get; set; }
		public DateTime ProcessedDate { get; set; }
		public List<FileDetail> FileDetails { get; set; } = new List<FileDetail>();
		public int NumberOfRecords { get; set; }
		public string Username { get; set; }
		public string Message { get; set; }
		public bool IsSuccessful { get; set; }
		public bool InProgress { get; set; }
		public DateTime LocalCreateDate { get; set; }
		public string TimeElapsed { get; set; }
		public string BulkResponse { get; set; }
	}
}