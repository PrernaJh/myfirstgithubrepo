using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IOperationalContainerRepository : IRepository<OperationalContainer>
	{
		Task<OperationalContainer> GetMostRecentOperationalContainerAsync(string siteName, string binCode, string partitionKeyString);
		Task<IEnumerable<OperationalContainer>> GetOutOfDateOperationalContainersAsync(string siteName, DateTime localTime, int localTimeOffset);
		Task<OperationalContainer> GetOperationalContainerAsync(string siteName, string containerId, string partitionKeyString);
		Task<OperationalContainer> GetActiveOperationalContainerAsync(string siteName, string binCode, string partitionKeyString);
	}
}
