using System;
using System.Collections.Generic;

namespace PackageTracker.Domain.Models.FileProcessing
{
	public class AsnFileImportResponse : FileImportResponse
	{
		public AsnFileImportResponse()
		{
			BadInputDocuments = new List<object>();
		}

		public TimeSpan DomainDataTotalTime { get; set; }
		public TimeSpan DuplicateCheckTotalTime { get; set; }
		public TimeSpan ActiveGroupsTotalTime { get; set; }
		public TimeSpan AssignUpsGeoDescTotalTime { get; set; }
		public TimeSpan AssignZipOverridesTotalTime { get; set; }
		public TimeSpan BinsTotalTime { get; set; }
		public TimeSpan SequencesTotalTime { get; set; }
		public TimeSpan ZonesTotalTime { get; set; }

		public string ZipOverrideAssignmentAverage { get; set; }
		public string BinsAverage { get; set; }
		public string SequencesAverage { get; set; }
		public string ZonesAverage { get; set; }
		public string DomainDataAverage { get; set; }
		public string DuplicateCheckAverage { get; set; }

		public int NumberOfDuplicates { get; set; }
		public string AverageDuplicatesPerPackage { get; set; }
	}
}