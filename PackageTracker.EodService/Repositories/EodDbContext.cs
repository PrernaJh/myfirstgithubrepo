using Microsoft.EntityFrameworkCore;
using PackageTracker.EodService.Data.Models;

namespace PackageTracker.EodService.Repositories
{
	public class EodDbContext : DbContext
	{
		public EodDbContext(DbContextOptions<EodDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}

		public DbSet<EodContainer> EodContainers { get; set; }
		public DbSet<EodPackage> EodPackages { get; set; }

		public DbSet<ContainerDetailRecord> ContainerDetailRecords { get; set; }
		public DbSet<EvsPackage> EvsContainers { get; set; }
		public DbSet<EvsPackage> EvsPackages { get; set; }
		public DbSet<InvoiceRecord> InvoiceRecords { get; set; }
		public DbSet<PackageDetailRecord> PackageDetailRecords { get; set; }
		public DbSet<PmodContainerDetailRecord> PmodContainerDetailRecords { get; set; }
		public DbSet<ReturnAsnRecord> ReturnAsnRecords { get; set; }
		public DbSet<ExpenseRecord> ExpenseRecords { get; set; }
	}
}
