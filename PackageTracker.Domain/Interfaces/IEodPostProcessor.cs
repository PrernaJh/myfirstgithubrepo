using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IEodPostProcessor
	{
		Task<bool> ShouldLockPackage(Package package);
		Task<bool> ShouldLockContainer(ShippingContainer container);
	}
}
