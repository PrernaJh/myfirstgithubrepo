using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Domain.Models
{
	public class ReportResponse
	{
		public List<object> BadInputDocuments { get; set; }
		public int NumberOfFailedDocuments { get; set; }
		public int NumberOfDocuments { get; set; }
		public string Message { get; set; }
		public bool IsSuccessful { get; set; }
		public string Name { get; set; }
	}
}
