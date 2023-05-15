using System.Threading.Tasks;

namespace ParcelPrepGov.DataUtility
{
	public interface IUspsEvsFileService
	{
		Task ExportUspsEvsFile(string message);
	}
}
