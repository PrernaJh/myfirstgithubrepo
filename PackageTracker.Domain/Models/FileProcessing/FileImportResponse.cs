using System;
using System.Collections.Generic;

namespace PackageTracker.Domain.Models.FileProcessing
{
	public class FileImportResponse
	{
		public FileImportResponse()
		{
			BadInputDocuments = new List<object>();
		}

		public List<object> BadInputDocuments { get; set; }
		public long NumberOfDocumentsImported { get; set; }
		public double RequestUnitsConsumed { get; set; }
		public TimeSpan FileReadTime { get; set; }
		public TimeSpan DbInsertTime { get; set; }
		public TimeSpan TotalTime { get; set; }
		public string Message { get; set; }
		public bool IsSuccessful { get; set; }
		public string Name { get; set; }
	}
}
