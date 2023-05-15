using Microsoft.EntityFrameworkCore;

namespace PpgReports.Data
{
    public class PpgReportsDbContext : DbContext
    {
        public DbSet<PackageDataset> PackageDatasets { get; set; }

        public PpgReportsDbContext(DbContextOptions<PpgReportsDbContext> options)
            : base(options)
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Core Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Core Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
