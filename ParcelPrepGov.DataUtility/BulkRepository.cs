using Microsoft.Extensions.Logging;
using PackageTracker.Data.Interfaces;

namespace ParcelPrepGov.DataUtility
{
	public class BulkRepository : BulkDbRepository
	{
		public BulkRepository(ILogger<BulkRepository> logger, IBulkDbClientFactory factory) : base(logger, factory)
		{ }
	}
}
