using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Domain.Models.FileProcessing
{
    public class FileReadResponse
    {
		public long NumberOfDocumentsRead { get; set; }
		public TimeSpan FileReadTime { get; set; }
		public string Message { get; set; }
		public bool IsSuccessful { get; set; }
	}
}
