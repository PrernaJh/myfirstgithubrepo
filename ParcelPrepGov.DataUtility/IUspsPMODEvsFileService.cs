using System.Threading.Tasks;

namespace ParcelPrepGov.DataUtility
{
	public interface IUspsPmodEvsFileService
	{
		Task ExportUspsEvsFileForPMODContainers(string message);
	}
}
