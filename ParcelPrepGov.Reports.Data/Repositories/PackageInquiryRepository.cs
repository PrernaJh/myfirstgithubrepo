using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Repositories
{
    public class PackageInquiryRepository : IPackageInquiryRepository
    {
        private readonly IPpgReportsDbContextFactory factory;
        private readonly ILogger<PackageInquiryRepository> logger;

        public PackageInquiryRepository(IPpgReportsDbContextFactory factory, ILogger<PackageInquiryRepository> logger)
        {
            this.factory = factory;
            this.logger = logger;
        }

        public async Task<PackageInquiry> GetPackageInquiryAsync(int packageDatasetId)
        {
            using (var context = factory.CreateDbContext())
            {
                return await context.PackageInquiries.AsNoTracking().FirstOrDefaultAsync(i => i.PackageDatasetId == packageDatasetId);
            }
        }
    }
}
