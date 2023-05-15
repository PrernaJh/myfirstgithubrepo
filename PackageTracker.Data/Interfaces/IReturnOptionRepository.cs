using PackageTracker.Data.Models.ReturnOptions;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface IReturnOptionRepository : IRepository<ReturnOption>
	{
		Task<ReturnOption> GetReturnOptionsBySiteAsync(string siteName);
	}
}
