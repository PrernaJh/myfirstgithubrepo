using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.AsnService
{
	public interface IAsnFileService
	{
		Task ProcessAsnFilesAsync(WebJobSettings webJobSettings);
	}
}
