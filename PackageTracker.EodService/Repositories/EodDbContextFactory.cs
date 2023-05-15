using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IEodDbContextFactory = PackageTracker.EodService.Interfaces.IEodDbContextFactory;

namespace PackageTracker.EodService.Repositories
{
    public class EodDbContextFactory : IDesignTimeDbContextFactory<EodDbContext>, IEodDbContextFactory
    {
        private readonly IConfiguration configuration;

        public EodDbContextFactory()
        {
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            configuration = builder.Build();
        }

        public EodDbContextFactory(IConfiguration configuration, ILogger<EodDbContextFactory> logger)
        {
            this.configuration = configuration;
        }

        public EodDbContext CreateDbContext()
        {
            return CreateDbContext(null);
        }

        public EodDbContext CreateDbContext(string[] args)
        {
            var connectionString = LoadConnectionString();
            var commandTimeout = LoadCommandTimeout();
            var builder = new DbContextOptionsBuilder<EodDbContext>();
            builder.UseSqlServer(connectionString,
                opts => opts.CommandTimeout(commandTimeout));
            var context = new EodDbContext(builder.Options);
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            return context;
        }

        private string LoadConnectionString()
        {
            return configuration.GetConnectionString("MmsEodDb");
        }
        private int LoadCommandTimeout()
        {
            if (!int.TryParse(configuration.GetSection("SqlCommandTimeout").Value, out var commandTimeout))
                commandTimeout = 300; // seconds

            return commandTimeout;
        }
    }
}
