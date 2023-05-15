using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParcelPrepGov.Reports.Interfaces;

namespace ParcelPrepGov.Reports.Data
{
    public class PpgReportsDbContextFactory : IDesignTimeDbContextFactory<PpgReportsDbContext>, IPpgReportsDbContextFactory
    {
        private readonly IConfiguration configuration;

        public PpgReportsDbContextFactory()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
            
            configuration = builder.Build();            
        }

        public PpgReportsDbContextFactory(IConfiguration configuration, ILogger<PpgReportsDbContextFactory> logger)
        {
            this.configuration = configuration;
        }

        public PpgReportsDbContext CreateDbContext()
        {
            return CreateDbContext(null);
        }

        public PpgReportsDbContext CreateDbContext(string[] args)
        {
            var connectionString = LoadConnectionString();
            var commandTimeout = LoadCommandTimeout();
            var builder = new DbContextOptionsBuilder<PpgReportsDbContext>();
            builder.UseSqlServer(connectionString,
                opts => opts.CommandTimeout(commandTimeout));
            var context = new PpgReportsDbContext(builder.Options);
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            return context;
        }

        private string LoadConnectionString()
        {
            return configuration.GetConnectionString("PpgReportsDb");
        }
        private int LoadCommandTimeout()
        {
            if (!int.TryParse(configuration.GetSection("SqlCommandTimeout").Value, out var commandTimeout))
                commandTimeout = 300; // seconds

            return commandTimeout;
        }
    }
}
