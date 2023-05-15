using PackageTracker.Domain.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System;
using System.Text;

namespace PackageTracker.Domain.Utilities
{
	public static class LogFileUtility
	{
		public static string LogFileImportResponse(string jobName, FileImportResponse response)
		{
			if (response != null)
			{
				var message = new StringBuilder();

				message.Append($"{ jobName } | Total records processed: { response.NumberOfDocumentsImported }" + Environment.NewLine);
				message.Append($"{ jobName } | Request units consumed: { response.RequestUnitsConsumed }" + Environment.NewLine);
				message.Append($"{ jobName } | Bad input documents: { response.BadInputDocuments.Count }" + Environment.NewLine);
				message.Append($"{ jobName } | File read time elapsed: { response.FileReadTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Bulk insert time elapsed: { response.DbInsertTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Total time elapsed: { response.TotalTime:mm\\:ss\\.ff}");

				return message.ToString();
			}
			return string.Empty;
		}

		public static string LogAsnFileImportResponse(string jobName, AsnFileImportResponse response)
		{
			if (response != null)
			{
				var message = new StringBuilder();

				message.Append($"{ jobName } | Total records processed: { response.NumberOfDocumentsImported }" + Environment.NewLine);
				message.Append($"{ jobName } | Request units consumed: { response.RequestUnitsConsumed }" + Environment.NewLine);
				message.Append($"{ jobName } | Bad input documents: { response.BadInputDocuments.Count }" + Environment.NewLine);
				message.Append($"{ jobName } | Active Groups seconds elapsed: { response.ActiveGroupsTotalTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Ups Geo Desc time elapsed: { response.AssignUpsGeoDescTotalTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Zip Overrides Assignment time elapsed: { response.AssignZipOverridesTotalTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Zip Overrides Assignment average ms per package: {response.ZipOverrideAssignmentAverage}" + Environment.NewLine);
				message.Append($"{ jobName } | Bins and Bin Maps time elapsed: { response.BinsTotalTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Bins and Bin Maps average ms per package: {response.BinsAverage}" + Environment.NewLine);
				message.Append($"{ jobName } | Sequences time elapsed: { response.SequencesTotalTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Sequences average ms per package: {response.SequencesAverage}" + Environment.NewLine);
				message.Append($"{ jobName } | Zones time elapsed: { response.ZonesTotalTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Zones average ms per package: {response.ZonesAverage}" + Environment.NewLine);
				message.Append($"{ jobName } | Duplicate check total time elapsed: { response.DuplicateCheckTotalTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Total number of duplicates: { response.NumberOfDuplicates}" + Environment.NewLine);
				message.Append($"{ jobName } | Average duplicates per package: { response.AverageDuplicatesPerPackage}" + Environment.NewLine);
				message.Append($"{ jobName } | Duplicate check average ms per package: {response.DuplicateCheckAverage}" + Environment.NewLine);
				message.Append($"{ jobName } | Domain data total time elapsed: { response.DomainDataTotalTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Domain data average ms per package: {response.ZonesAverage}" + Environment.NewLine);
				message.Append($"{ jobName } | Total data assignment time elapsed: { response.TotalTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Bulk insert time elapsed: { response.DbInsertTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Total time elapsed: { response.TotalTime:mm\\:ss\\.ff}");

				return message.ToString();
			}
			return string.Empty;
		}

		public static string LogFileExportResponse(string jobName, string fileName, FileExportResponse response)
		{
			if (response != null)
			{
				var message = new StringBuilder();

				message.Append($"{ jobName } | Created file: { fileName }" + Environment.NewLine);
				message.Append($"{ jobName } | Total records processed: { response.FileContents.Count }" + Environment.NewLine);
				message.Append($"{ jobName } | Bad output documents: { response.BadOutputDocuments.Count }" + Environment.NewLine);
				message.Append($"{ jobName } | Db read time elapsed: { response.DbReadTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Db write time elapsed: { response.DbWriteTime:mm\\:ss\\.ff}" + Environment.NewLine);
				message.Append($"{ jobName } | Total time elapsed: { response.TotalTime:mm\\:ss\\.ff}");

				return message.ToString();
			}
			return string.Empty;
		}
		public static string LogXmlImportResponse(string jobName, XmlImportResponse response, TimeSpan totalTime)
		{
			var message = new StringBuilder();

			message.Append($"{ jobName } | Total records processed: { response.NumberOfDocumentsImported }" + Environment.NewLine);
			message.Append($"{ jobName } | Request units consumed: { response.RequestUnitsConsumed }" + Environment.NewLine);
			message.Append($"{ jobName } | Bad input documents: { response.BadInputDocuments.Count }" + Environment.NewLine);
			message.Append($"{ jobName } | http Request duration: { response.HttpRequestDuration:mm\\:ss\\.ff}" + Environment.NewLine);
			message.Append($"{ jobName } | Json serialize time elapsed: { response.SerializeTime:mm\\:ss\\.ff}" + Environment.NewLine);
			message.Append($"{ jobName } | Bulk insert time elapsed: { response.DbInsertTime:mm\\:ss\\.ff}" + Environment.NewLine);
			message.Append($"{ jobName } | Total time elapsed: { totalTime:mm\\:ss\\.ff}");

			return message.ToString();
		}
	}
}
