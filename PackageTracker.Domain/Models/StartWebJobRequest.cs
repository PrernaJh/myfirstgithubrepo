using PackageTracker.Data.Models;
using System;

namespace PackageTracker.Domain.Models
{
	public class StartWebJobRequest
	{
		public Site Site { get; set; }
		public DateTime ProcessedDate { get; set; }
		public string SubClientName { get; set; }
		public string WebJobTypeConstant { get; set; }
		public string JobName { get; set; }
		public string Message { get; set; }
		public string Username { get; set; }
	}
}
