using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
	public interface IPackageServiceProcessor
	{
		Task<bool> GetServiceDataAsync(Package package);
	}
}