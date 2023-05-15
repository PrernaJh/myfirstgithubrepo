using ParcelPrepGov.Reports.Data;

namespace ParcelPrepGov.Reports.Interfaces
{
	public interface IPpgReportsDbContextFactory
	{
		PpgReportsDbContext CreateDbContext();
		PpgReportsDbContext CreateDbContext(string[] args);
	}
}
