using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Service.Interfaces
{
	public interface IUpsGeoDescFileService
	{
		Task ProcessUpsGeoDescFileAsync(Stream fileStream, string fileName);
	}
}
