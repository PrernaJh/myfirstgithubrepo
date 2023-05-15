using PackageTracker.Data.Models;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IZipMapProcessor
	{
		Task<ZipMap> GetZipMapAsync(string activeGroupType, string zipCode);
		Task<ZipMap> GetZipMapByGroupAsync(string activeGroupId, string zipCode);
	}
}
