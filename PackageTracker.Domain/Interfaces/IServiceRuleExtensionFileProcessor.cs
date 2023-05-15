using PackageTracker.Domain.Models.FileProcessing;
using System.IO;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IServiceRuleExtensionFileProcessor
	{
		Task<FileImportResponse> ImportFortyEightStatesFileToDatabase(Stream fileStream, string subClientName);
	}
}
