using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Service.Interfaces
{
	public interface IZoneFileService
	{
		Task ProcessZoneFileAsync(Stream fileStream, string fileName);
	}
}
