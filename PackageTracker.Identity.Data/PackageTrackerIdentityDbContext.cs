using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PackageTracker.Identity.Data.Models;

namespace PackageTracker.Identity.Data
{
	public class PackageTrackerIdentityDbContext : IdentityDbContext<ApplicationUser>
	{
		public PackageTrackerIdentityDbContext(DbContextOptions<PackageTrackerIdentityDbContext> options)
			: base(options)
		{
			//Database.Migrate();
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Override default AspNet Identity table names
			modelBuilder.Entity<ApplicationUser>(entity => { entity.ToTable(name: "Users"); });
			modelBuilder.Entity<IdentityRole>(entity => { entity.ToTable(name: "Roles"); });
			modelBuilder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("UserRoles"); });
			modelBuilder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("UserClaims"); });
			modelBuilder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("UserLogins"); });
			modelBuilder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("UserTokens"); });
			modelBuilder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("RoleClaims"); });
		}
	}
}
