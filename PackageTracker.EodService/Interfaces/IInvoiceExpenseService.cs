using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
	public interface IInvoiceExpenseService
	{
		Task ProcessInvoiceFiles(WebJobSettings webJobSettings, string message);
		Task ProcessExpenseFiles(WebJobSettings webJobSettings, string message);
		Task ProcessPeriodicInvoiceFiles(WebJobSettings webJobSettings);
		Task ProcessPeriodicExpenseFiles(WebJobSettings webJobSettings);
	}
}
