using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.Service.Interfaces
{
	public interface IDuplicateAsnCheckerService
	{
		Task CheckForDuplicateAsns(WebJobSettings webJobSettings);
	}
}
