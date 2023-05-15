using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
    public class BinProcessor : IBinProcessor
    {
        private readonly IActiveGroupProcessor activeGroupProcessor;
        private readonly IBinRepository binRepository;
        private readonly IBinMapRepository binMapRepository;
        private readonly ILogger<BinProcessor> logger;

        public BinProcessor(IActiveGroupProcessor activeGroupProcessor, IBinRepository binRepository, IBinMapRepository binMapRepository, ILogger<BinProcessor> logger)
        {
            this.activeGroupProcessor = activeGroupProcessor;
            this.binMapRepository = binMapRepository;
            this.binRepository = binRepository;
            this.logger = logger;
        }

        public async Task<List<Bin>> GetBinsByActiveGroupIdAsync(string activeGroupId)
        {
            var response = await binRepository.GetBinsByActiveGroupIdAsync(activeGroupId);
            return response.ToList();
        }

        public async Task<List<Bin>> GetBinCodesAsync(string activeGroupId)
        {
            var response = await binRepository.GetBinCodesAsync(activeGroupId);
            return response.ToList();
        }

        public async Task<Bin> GetBinByBinCodeAsync(string binCode, string activeGroupId)
        {
            return await binRepository.GetBinByBinCodeAsync(binCode, activeGroupId);
        }

        public async Task AssignPackageToCurrentBinAsync(Package package)
        {
            var binActiveGroupId = await activeGroupProcessor.GetBinActiveGroupIdAsync(package.SiteName, package.TimeZone);
            var binMapActiveGroupId = await activeGroupProcessor.GetBinMapActiveGroupIdAsync(package.SubClientName, package.TimeZone);

            await AssignPackageBinAndBinMap(package, binActiveGroupId, binMapActiveGroupId);
        }

        public async Task AssignCreatePackageBinAsync(Package package, bool isInitialCreate, int daysPlus = 0)
        {
            var dateTimeToQuery = DateTime.MinValue;

            if (isInitialCreate)
            {
                var shipDateTime = package.ShipDate ?? DateTime.MinValue;
                dateTimeToQuery = shipDateTime.AddDays(daysPlus + 1);
            }
            else
            {
                dateTimeToQuery = TimeZoneUtility.GetLocalTime(package.TimeZone);
            }

            if (package.BinRuleType == CreatePackageConstants.ClientRuleTypeConstant)
            {
                var binActiveGroupId = await activeGroupProcessor.GetBinActiveGroupIdByDateAsync(package.SiteName, dateTimeToQuery, package.TimeZone);
                var bin = await binRepository.GetBinByBinCodeAsync(package.BinCode, binActiveGroupId);

                if (StringHelper.Exists(bin.Id))
                {
                    package.BinGroupId = binActiveGroupId;
                    package.IsDduScfBin = true;
                    package.IsAptbBin = bin.IsAptb;
                    package.IsScscBin = bin.IsScsc;
                }
                else
                {
                    logger.LogError($"Invalid Bin Code {package.BinCode} in CreatePackageRequest for Package ID: {package.PackageId}");
                    package.BinCode = string.Empty;
                }
            }
            else if (package.BinRuleType == CreatePackageConstants.SystemRuleTypeConstant)
            {
                package.BinCode = string.Empty;
                var binActiveGroupId = await activeGroupProcessor.GetBinActiveGroupIdByDateAsync(package.SiteName, dateTimeToQuery, package.TimeZone);
                var binMapActiveGroupId = await activeGroupProcessor.GetBinMapActiveGroupIdByDateAsync(package.SubClientName, dateTimeToQuery, package.TimeZone);
                await AssignPackageBinAndBinMap(package, binActiveGroupId, binMapActiveGroupId);
            }
        }

        public async Task<bool> VerifyCreatedPackageBinOnScan(Package package)
        {
            var isVerified = false;
            var currentDateToQuery = TimeZoneUtility.GetLocalTime(package.TimeZone);
            var binActiveGroupId = await activeGroupProcessor.GetBinActiveGroupIdByDateAsync(package.SiteName, currentDateToQuery, package.TimeZone);
            var binMapActiveGroupId = await activeGroupProcessor.GetBinMapActiveGroupIdByDateAsync(package.SubClientName, package.ShipDate, package.TimeZone);
            var currentBinCode = string.Empty;
            var zipBinMap5Digit = await GetBinMapByFiveDigitZip(package.Zip, binMapActiveGroupId);

            if (StringHelper.Exists(zipBinMap5Digit.BinCode))
            {
                currentBinCode = zipBinMap5Digit.BinCode;
            }

            if (StringHelper.DoesNotExist(currentBinCode))
            {
                var zipBinMap3Digit = await GetBinMapByThreeDigitZip(AddressUtility.TrimZipToFirstThree(package.Zip), binMapActiveGroupId);

                if (StringHelper.Exists(zipBinMap3Digit.BinCode))
                {
                    currentBinCode = zipBinMap3Digit.BinCode;
                }
            }
            if (StringHelper.DoesNotExist(currentBinCode))
            {
                logger.LogError($"Failed to find current BinCode for packageId: {package.PackageId}");
            }
            else if (currentBinCode != package.BinCode)
            {
                var bin = await binRepository.GetBinByBinCodeAsync(currentBinCode, binActiveGroupId);

                if (StringHelper.Exists(bin.Id))
                {
                    package.HistoricalBinCodes.Add(package.BinCode ?? string.Empty);
                    package.HistoricalBinGroupIds.Add(package.BinGroupId ?? string.Empty);
                    package.HistoricalBinMapGroupIds.Add(package.BinMapGroupId ?? string.Empty);
                    package.BinCode = bin.BinCode;
                    package.IsDduScfBin = true;
                    package.IsAptbBin = bin.IsAptb;
                    package.IsScscBin = bin.IsScsc;
                }
            }
            else
            {
                isVerified = true;
            }

            return isVerified;
        }        

        public async Task AssignBinsForListOfPackagesAsync(List<Package> packages, string binActiveGroupId, string binMapActiveGroupId, bool isUpdate = false)
        {
            AssignBinAndBinMapGroupIds(packages, binActiveGroupId, binMapActiveGroupId, isUpdate);
            await AssignBinCodeFor5DigitZips(packages, binMapActiveGroupId); // look for 5 digit binCode first
            await AssignBinCodeFor3DigitZips(packages, binMapActiveGroupId);
            await AssignBinFlags(packages, binActiveGroupId);
        }

        private async Task AssignPackageBinAndBinMap(Package package, string binActiveGroupId, string binMapActiveGroupId, bool isHistorical = false)
        {
            if (isHistorical)
            {
                package.HistoricalBinCodes.Add(package.BinCode);
                package.HistoricalBinGroupIds.Add(package.BinGroupId);
                package.HistoricalBinMapGroupIds.Add(package.BinMapGroupId);
            }

            package.BinMapGroupId = binMapActiveGroupId;
            package.BinGroupId = binActiveGroupId;

            var zipBinMap5Digit = await GetBinMapByFiveDigitZip(package.Zip, binMapActiveGroupId);

            if (StringHelper.Exists(zipBinMap5Digit.BinCode))
            {
                package.BinCode = zipBinMap5Digit.BinCode;
                package.IsDduScfBin = true;
            }
            else
            {
                package.BinCode = string.Empty;
                package.IsDduScfBin = false;
            }

            if (StringHelper.DoesNotExist(package.BinCode))
            {
                var zipBinMap3Digit = await GetBinMapByThreeDigitZip(AddressUtility.TrimZipToFirstThree(package.Zip), binMapActiveGroupId);

                if (StringHelper.Exists(zipBinMap3Digit.BinCode))
                {
                    package.BinCode = zipBinMap3Digit.BinCode;
                    package.IsDduScfBin = true;
                }
                else
                {
                    package.BinCode = string.Empty;
                    package.IsDduScfBin = false;
                }
            }

            if (StringHelper.Exists(package.BinCode))
            {
                var bin = await binRepository.GetBinByBinCodeAsync(package.BinCode, binActiveGroupId);

                if (StringHelper.Exists(bin.Id))
                {
                    package.IsAptbBin = bin.IsAptb;
                    package.IsScscBin = bin.IsScsc;
                }
            }
        }

        private async Task<BinMap> GetBinMapByThreeDigitZip(string zip, string activeGroupId)
        {
            return await binMapRepository.GetBinMapByZip(AddressUtility.TrimZipToFirstThree(zip), activeGroupId);
        }

        private async Task<BinMap> GetBinMapByFiveDigitZip(string zip, string activeGroupId)
        {
            return await binMapRepository.GetBinMapByZip(zip, activeGroupId);
        }

        private static void AssignBinAndBinMapGroupIds(List<Package> packages, string binActiveGroupId, string binMapActiveGroupId, bool isUpdate)
        {
            foreach (var package in packages)
            {
                if (isUpdate)
                {
                    package.HistoricalBinCodes.Add(package.BinCode);
                    package.HistoricalBinGroupIds.Add(package.BinGroupId);
                    package.HistoricalBinMapGroupIds.Add(package.BinMapGroupId);
                }

                package.BinMapGroupId = binMapActiveGroupId;
                package.BinGroupId = binActiveGroupId;
            }
        }

        private async Task AssignBinCodeFor5DigitZips(List<Package> packages, string binMapActiveGroupId)
        {
            var groupedPackagesFiveDigit = packages.GroupBy(x => x.Zip);

            foreach (var groupOfPackages in groupedPackagesFiveDigit)
            {
                var zipBinMap = await GetBinMapByFiveDigitZip(groupOfPackages.Key, binMapActiveGroupId);

                foreach (var package in groupOfPackages)
                {
                    if (StringHelper.Exists(zipBinMap.BinCode))
                    {
                        package.BinCode = zipBinMap.BinCode;
                        package.IsDduScfBin = true;
                    }
                    else
                    {
                        package.BinCode = string.Empty;
                        package.IsDduScfBin = false;
                    }
                }
            }
        }

        private async Task AssignBinCodeFor3DigitZips(List<Package> packages, string binMapActiveGroupId)
        {
            var groupedPackagesThreeDigit = packages.Where(x => StringHelper.DoesNotExist(x.BinCode)).GroupBy(y => AddressUtility.TrimZipToFirstThree(y.Zip));

            foreach (var groupOfPackages in groupedPackagesThreeDigit)
            {
                var zipBinMap = await GetBinMapByThreeDigitZip(groupOfPackages.Key, binMapActiveGroupId);

                foreach (var package in groupOfPackages)
                {
                    if (StringHelper.Exists(zipBinMap.BinCode))
                    {
                        package.BinCode = zipBinMap.BinCode;
                        package.IsDduScfBin = true;
                    }
                    else
                    {
                        package.BinCode = string.Empty;
                        package.IsDduScfBin = false;
                    }
                }
            }
        }

        private async Task AssignBinFlags(List<Package> packages, string binActiveGroupId)
        {
            var groupedPackagesBinCode = packages.GroupBy(x => x.BinCode);

            foreach (var groupOfPackages in groupedPackagesBinCode.Where(x => StringHelper.Exists(x.Key)))
            {
                var bin = await binRepository.GetBinByBinCodeAsync(groupOfPackages.Key, binActiveGroupId);

                if (StringHelper.Exists(bin.Id))
                {
                    foreach (var package in groupOfPackages)
                    {
                        package.IsAptbBin = bin.IsAptb;
                        package.IsScscBin = bin.IsScsc;
                    }
                }
            }
        }
    }
}
