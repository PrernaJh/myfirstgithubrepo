using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace MMS.API.Domain.Interfaces
{
	public interface IPackageRepeatProcessor
	{
		Task<Package> ProcessRepeatScan(Package packageToReplace, string username, string machineId);
	}
}
