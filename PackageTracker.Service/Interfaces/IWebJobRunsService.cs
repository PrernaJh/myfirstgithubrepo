using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.Service.Interfaces
{
	public interface IWebJobRunsService
	{
		Task MonitorRecentAsnImportsAsync(WebJobSettings webJobSettings);
	}
}
