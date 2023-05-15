using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
	public interface IReturnAsnService
	{
		Task ExportReturnAsnFiles(WebJobSettings webJobSettings, string message);
	}
}
