using System.Threading.Tasks;

namespace PackageTracker.EodService.Interfaces
{
	public interface IContainerDetailService
	{
		Task ExportContainerDetailFile(string message);
		Task ExportPmodContainerDetailFile(string message);
	}
}
