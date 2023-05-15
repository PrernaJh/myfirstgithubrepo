using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.Data.Interfaces
{
	public interface ISequenceRepository : IRepository<Sequence>
	{
		Task<Sequence> GetSequenceAsync(string siteName, string sequenceType);
	}
}
