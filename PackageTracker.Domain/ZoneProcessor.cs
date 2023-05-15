using Microsoft.Extensions.Logging;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
    public class ZoneProcessor : IZoneProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly IZoneMapRepository zoneMapRepository;
        private readonly ILogger<ZoneProcessor> logger;

        public ZoneProcessor(IActiveGroupProcessor activeGroupProcessor, IZoneMapRepository zoneMapRepository, ILogger<ZoneProcessor> logger)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.zoneMapRepository = zoneMapRepository;
            this.logger = logger;
        }

        public async Task AssignZonesToListOfPackages(List<Package> packages, Site site, string zoneMapGroupId)
        {
            var groupedPackages = packages.GroupBy(x => AddressUtility.TrimZipToFirstThree(x.Zip));

            foreach (var groupOfPackages in groupedPackages)
            {
                var positionParsed = int.TryParse(groupOfPackages.Key, out var matrixPosition);
                if (groupOfPackages.Key.Length == 3 && Regex.IsMatch(groupOfPackages.Key, "[0-9]+") && matrixPosition != 0)
                {
                    var zoneMap = await zoneMapRepository.GetZoneMapAsync(AddressUtility.TrimZipToFirstThree(site.Zip), zoneMapGroupId);
                    var zones = StringHelper.SplitInParts(zoneMap.ZoneMatrix, 2).ToList();
                    var trueMatrixPosition = matrixPosition - 1; // minus 1 because the zone matrix is not zero based, but the input list is

                    var zoneParsed = int.TryParse(zones[trueMatrixPosition].ToString().Substring(0, 1), out var zone);

                    if (positionParsed && zoneParsed)
                    {
                        foreach (var package in groupOfPackages)
                        {
                            package.Zone = zone;
                        }
                    }
                    else
                    {
                        logger.Log(LogLevel.Error, $"Failed to assign Zone on import. ZoneMap Id: {zoneMap.Id}");
                    }
                }
                else
                {
                    logger.Log(LogLevel.Error, $"Failed to assign Zone on import. Zip is invalid: {groupOfPackages.Key}");
                }
            }
        }

        public async Task AssignZone(Package package)
        {
            var zoneMapGroupId = await activeGroupProcessor.GetZoneMapActiveGroupIdAsync();

            if (StringHelper.Exists(zoneMapGroupId))
            {
                package.ZoneMapGroupId = zoneMapGroupId;
                var zoneMap = await zoneMapRepository.GetZoneMapAsync(AddressUtility.TrimZipToFirstThree(package.SiteZip), zoneMapGroupId);

                if (StringHelper.Exists(zoneMap.Id))
                {
                    var zones = StringHelper.SplitInParts(zoneMap.ZoneMatrix, 2).ToList();
                    var positionParsed = int.TryParse(AddressUtility.TrimZipToFirstThree(package.Zip), out var matrixPosition);
                    var trueMatrixPosition = matrixPosition - 1; // minus 1 because the zone matrix is not zero based, but the input list is

                    var zoneParsed = int.TryParse(zones[trueMatrixPosition].ToString().Substring(0, 1), out var zone);

                    if (positionParsed && zoneParsed)
                    {
                        package.Zone = zone;
                    }
                }
            }
        }
    }
}
