using Microsoft.EntityFrameworkCore;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Repositories
{
    public class ReportingRepository : IReportingRepository
    {
        private readonly IPpgReportsDbContextFactory factory;

        public ReportingRepository(IPpgReportsDbContextFactory factory)
        {
            this.factory = factory;
        }
        async Task<List<Report>> IReportingRepository.GetAllReportsAsync()
        {
            using (var context = factory.CreateDbContext())
            {
                return await context.Reports.ToListAsync();
            }
        }
        async Task<Report> IReportingRepository.GetReportByNameAsync(string reportName)
        {
            using (var context = factory.CreateDbContext())
            {
                return await context.Reports.FirstOrDefaultAsync(r => r.ReportName == reportName);
            }
        }

        async Task<Report> IReportingRepository.InsertReport(Report report)
        {
            using (var context = factory.CreateDbContext())
            {
                var now = DateTime.Now;
                report.CreateDate = now;
                report.ChangeDate = now;
                context.Add(report);
                await context.SaveChangesAsync();
                
                return report;
            }
        }

        async Task<Report> IReportingRepository.UpdateReport(Report report)
        {
            using (var context = factory.CreateDbContext())
            {
                var now = DateTime.Now;
                report.ChangeDate = now;
                context.Update(report);
                await context.SaveChangesAsync();
                return report;
            }
        }

        async Task<Report> IReportingRepository.RemoveReport(Report report)
        {
            using (var context = factory.CreateDbContext())
            {
                context.Remove(report);
                await context.SaveChangesAsync();
                return report;
            }
        }
    }
}
