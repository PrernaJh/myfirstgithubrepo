using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
	public interface IEvsFileService
	{
		Task ExportEvsFile(string message);
		Task ExportPmodEvsFile(string message);
	}
}

