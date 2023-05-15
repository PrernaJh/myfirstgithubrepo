using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
    public interface IReportingRepository
    {
        public Task<List<Report>> GetAllReportsAsync();
        public Task<Report> GetReportByNameAsync(string reportName);
        public Task<Report> UpdateReport(Report report);
        public Task<Report> InsertReport(Report report);
        public Task<Report> RemoveReport(Report report);
    }
}
