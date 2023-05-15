using Microsoft.EntityFrameworkCore;
using ParcelPrepGov.Reports.Models;

namespace ParcelPrepGov.Reports.Data
{
	public class PpgReportsDbContext : DbContext
	{
		//private readonly IConfiguration configuration;

		//public PpgReportsDbContext() { }

		//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		//{
		//    optionsBuilder.UseSqlServer(_connString);
		//}

		//private readonly string _connString = "Server=tcp:tecmailing-packagetracker.database.usgovcloudapi.net,1433;Initial Catalog=ppgreports;Persist Security Info=False;User ID=tecmailing;Password=Password123$;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

		public PpgReportsDbContext(DbContextOptions<PpgReportsDbContext> options/*, IConfiguration configuration*/)
			: base(options)
		{
			//Database.Migrate();
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}

		public DbSet<Dashboard> Dashboards { get; set; }
		public DbSet<Report> Reports { get; set; }

		public DbSet<PostalAreaAndDistrict> PostalAreasAndDistricts { get; set; }
		public DbSet<PostalDays> PostalDays { get; set; }
		public DbSet<VisnSite> VisnSites { get; set; }
		public DbSet<EvsCode> EvsCodes { get; set; }
		public DbSet<RecallStatus> RecallStatuses { get; set; }

		public DbSet<BinDataset> BinDatasets { get; set; }
		public DbSet<JobDataset> JobDatasets { get; set; }
		public DbSet<JobContainerDataset> JobContainerDatasets { get; set; }
		public DbSet<PackageDataset> PackageDatasets { get; set; }
		public DbSet<PackageEventDataset> PackageEventDatasets { get; set; }
		public DbSet<PackageInquiry> PackageInquiries { get; set; }
		public DbSet<ShippingContainerDataset> ShippingContainerDatasets { get; set; }
		public DbSet<ShippingContainerEventDataset> ShippingContainerEventDatasets { get; set; }
		public DbSet<SubClientDataset> SubClientDatasets { get; set; }
		public DbSet<TrackPackageDataset> TrackPackageDatasets { get; set; }
		public DbSet<UndeliverableEventDataset> UndeliverableEventDatasets { get; set; }
		public DbSet<WebJobRunDataset> WebJobRunDatasets { get; set; }
		public DbSet<UserLookup> UserLookups { get; set; }

		public DbSet<CarrierEventCode> CarrierEventCodes { get; set; }
	}
}
