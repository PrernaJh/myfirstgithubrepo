using OfficeOpenXml;
using System;
using System.Collections.Generic;

namespace PackageTracker.Domain.Models.FileProcessing
{
	public class FileExportResponse
	{
		public FileExportResponse()
		{
			FileContents = new List<string>();
			BadOutputDocuments = new List<object>();
		}

		public string FileName { get; set; }
		public List<string> FileContents { get; set; }
		public List<object> BadOutputDocuments { get; set; }
		public TimeSpan DbReadTime { get; set; }
		public TimeSpan DbWriteTime { get; set; }
		public TimeSpan TotalTime { get; set; }
		public int NumberOfRecords { get; set; }
		public bool IsSuccessful { get; set; }
	}
}
