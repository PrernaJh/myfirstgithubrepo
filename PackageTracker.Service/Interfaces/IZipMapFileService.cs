using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Service.Interfaces
{
	public interface IZipMapFileService
	{
		Task ProcessZipMapFileAsync(Stream fileStream, string fileName);
	}
}
