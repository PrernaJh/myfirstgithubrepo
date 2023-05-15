using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IConsumerDetailFileProcessor
	{
		Task<FileExportResponse> GetConsumerDetailFileAsync(SubClient subClient, string webJobId, DateTime lastScanDateTime, DateTime nextScanDateTime);
		void CreateConsumerDetailRecords(FileExportResponse response, IEnumerable<Package> packages, SubClient subClient, string webJobId);
	}
}
