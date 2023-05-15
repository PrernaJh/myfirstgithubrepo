using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
	public interface IPackageDetailService
	{
		Task ExportPackageDetailFile(WebJobSettings webJobSettings, string message);
	}
}
