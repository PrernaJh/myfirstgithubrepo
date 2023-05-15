using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
	public class DatasetProcessor : IDatasetProcessor
	{
		private readonly IContainerRepository containerRepository;
		private readonly IPackageRepository packageRepository;
		private readonly IJobRepository jobRepository;

		public DatasetProcessor(IContainerRepository containerRepository, IJobRepository jobRepository, IPackageRepository packageRepository)
		{
			this.containerRepository = containerRepository;
			this.jobRepository = jobRepository;
			this.packageRepository = packageRepository;
		}

		public async Task<IEnumerable<ShippingContainer>> GetContainersForContainerDatasetsAsync(string siteName, DateTime lastScanDateTime)
		{
			return await containerRepository.GetContainersForContainerDatasetsAsync(siteName, lastScanDateTime);
		}

		public async Task<IEnumerable<Job>> GetJobsForJobDatasetsAsync(string siteName)
		{
			return (await jobRepository.GetJobsForJobDatasetsAsync(siteName))
				.Where(j => j.JobEvents.Any(x => x.EventStatus == EventConstants.JobStarted));
		}

		public async Task<IEnumerable<Package>> GetPackagesForPackageDatasetsAsync(string siteName, int firstTimestamp, int lastTimeStamp)
		{
			return await packageRepository.GetPackagesForPackageDatasetsAsync(siteName, firstTimestamp, lastTimeStamp);
		}

		public async Task<IEnumerable<int>> GetPackageTimeStampsAsync(string siteName, DateTime lastScanDateTime, DateTime nextScanDateTime)
		{
			return await packageRepository.GetPackageTimeStampsAsync(siteName, lastScanDateTime, nextScanDateTime);
		}	
	}
}
