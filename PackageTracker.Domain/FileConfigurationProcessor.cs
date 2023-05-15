using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
    public class FileConfigurationProcessor : IFileConfigurationProcessor
    {
        private readonly IFileConfigurationRepository fileConfigurationRepository;

        public FileConfigurationProcessor(IFileConfigurationRepository fileConfigurationRepository)
        {
            this.fileConfigurationRepository = fileConfigurationRepository;
        }

        public async Task<List<FileConfiguration>> GetAllEndOfDayFileConfigurationsAsync()
        {
            var response = await fileConfigurationRepository.GetAllFileConfigurations(SiteConstants.AllSites, FileConstants.EndOfDayScheduleType);
            return response.ToList();
        }
    }
}
