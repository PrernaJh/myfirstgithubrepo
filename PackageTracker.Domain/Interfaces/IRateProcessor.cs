using PackageTracker.Data.Models;
using PackageTracker.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IRateProcessor
	{
		Task<List<Rate>> GetCurrentRatesAsync(string subClientName);		
		Task<List<Package>> AssignPackageRatesForEod(SubClient subClient, List<Package> packages, string webJobId);
		Task<List<ShippingContainer>> AssignContainerRatesForEod(Site site, List<ShippingContainer> containers, string webJobId);
	}
}
