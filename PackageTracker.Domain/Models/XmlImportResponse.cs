using System;
using System.Collections.Generic;

namespace PackageTracker.Domain.Models
{
	public class XmlImportResponse
	{
		public XmlImportResponse()
		{
			BadInputDocuments = new List<object>();
		}

		public string Bookmark { get; set; }
		public List<object> BadInputDocuments { get; set; }
		public long NumberOfDocumentsImported { get; set; }
		public double RequestUnitsConsumed { get; set; }
		public TimeSpan HttpRequestDuration { get; set; }
		public TimeSpan DbInsertTime { get; set; }
		public TimeSpan SerializeTime { get; set; }
		public string Message { get; set; }
		public bool IsSuccessful { get; set; }
		public bool IsCompleted { get; set; }
	}
}
