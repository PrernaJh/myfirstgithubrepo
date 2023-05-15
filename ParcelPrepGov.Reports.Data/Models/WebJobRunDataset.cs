using System;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Reports.Models
{
	public class WebJobRunDataset : Dataset
	{
		[StringLength(30)]
		public string ClientName { get; set; }
		[StringLength(30)]
		public string SubClientName { get; set; }
		[StringLength(100)]
		public string JobName { get; set; }
		[StringLength(30)]
		public string JobType { get; set; }
		public DateTime ProcessedDate { get; set; }
		[StringLength(100)]
		public string FileName { get; set; }
		[StringLength(100)]
		public string FileArchiveName { get; set; }
		public int NumberOfRecords { get; set; }
		[StringLength(30)]
		public string Username { get; set; }
		public DateTime LocalCreateDate { get; set; }
		public bool IsSuccessful { get; set; }
		[StringLength(100)]
		public string Message { get; set; }
	}
}
