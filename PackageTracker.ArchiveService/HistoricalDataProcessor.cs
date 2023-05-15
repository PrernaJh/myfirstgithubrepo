using Microsoft.Extensions.Logging;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.ArchiveService.Interfaces;
using PackageTracker.Domain.Utilities;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using PackageTracker.Domain.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models.Archive;
using ParcelPrepGov.Reports.Interfaces;

namespace PackageTracker.ArchiveService
{
    public class HistoricalDataProcessor : IHistoricalDataProcessor
    {
        private readonly ILogger<HistoricalDataProcessor> logger;
        private readonly IActiveGroupRepository activeGroupRepository;
        private readonly IBinProcessor binProcessor;
        private readonly IPostalDaysRepository postalDaysRepository;
        private readonly ISiteProcessor siteProcessor;
        private readonly ISubClientProcessor subClientProcessor;


        public HistoricalDataProcessor(ILogger<HistoricalDataProcessor> logger,
            IActiveGroupRepository activeGroupRepository,
            IBinProcessor binProcessor,
            IPostalDaysRepository postalDaysRepository,
            ISiteProcessor siteProcessor,
            ISubClientProcessor subClientProcessor
            )
        {
            this.logger = logger;
            this.activeGroupRepository = activeGroupRepository;
            this.binProcessor = binProcessor;
            this.postalDaysRepository = postalDaysRepository;
            this.siteProcessor = siteProcessor;
            this.subClientProcessor = subClientProcessor;
        }

        private static string[] columnHeaders = {
            "Location",
            "Manifest Date",
            "Entry Unit Name",
            "Entry Unit State",
            "Entry Unit Zip",
            "Entry Unit Type",
            "Product",
            "Carrier",
            "Package ID",
            "Tracking Number",
            "Weight",
            "Last Known Status Date",
            "Last Known Status Time",
            "Last Known Status Desc",
            "Last Known Status Location",
            "Days To Last Known Status"
        };

        public async Task<(FileReadResponse readRespose, List<PackageForArchive> packages, ExcelWorkSheet exceptions)>
            ImportHistoricalDataFileAsync(WebJobSettings webJobSettings, Stream stream, DateTime fileDate)
        {
            var fileReadTime = Stopwatch.StartNew();
            var response = new FileReadResponse();
            var packages = new List<PackageForArchive>();
            ExcelWorkSheet exceptions = null;
            try
            {
                var sites = (await siteProcessor.GetAllSitesAsync()).ToList();
                var activeBins = new Dictionary<string, List<Bin>>();
                foreach (var site in sites)
                {
                    var groups = await activeGroupRepository.GetActiveGroupsByTypeAsync(ActiveGroupTypeConstants.Bins, site.SiteName);
                    var activeBinGroupId = string.Empty;
                    foreach (var group in groups.OrderBy(g => g.StartDate))
                    {
                        if (group.StartDate >= fileDate)
                            break;
                        activeBinGroupId = group.Id;
                    }

                    var bins = await binProcessor.GetBinsByActiveGroupIdAsync(activeBinGroupId);
                    foreach (var bin in bins)
                    {
                        var zip = AddressUtility.ParseCityStateZip(bin.DropShipSiteCszPrimary).FullZip;
                        if (zip.Length > 5)
                            zip = zip.Substring(0, 5);
                        bin.DropShipSiteCszPrimary = zip; // Save zip for alternate match
                    }
                    activeBins.Add(site.SiteName, bins);
                }
                var subClients = await subClientProcessor.GetSubClientsAsync();
                using (var ws = new ExcelWorkSheet(stream, columnHeaders))
                {
                    exceptions = new ExcelWorkSheet("Exceptions", ws.GetRow(ws.HeaderRow));
                    for (int row = ws.HeaderRow+1; row <= ws.RowCount; row++)
                    {
                        var package = ParseRow(ws, row, sites, subClients, activeBins);
                        if (package != null)
                        {
                            if (StringHelper.DoesNotExist(package.BinCode))
                            {
                                exceptions.InsertRow(exceptions.RowCount + 1, ws.GetRow(row));
                            }
                            packages.Add(package);
                        }
                        else
                        {
                            exceptions.InsertRow(exceptions.RowCount + 1, ws.GetRow(row));
                        }
                    }
                }
                if (exceptions.RowCount <= exceptions.HeaderRow)
                {
                    exceptions.Dispose();
                    exceptions = null; // No exceptions
                }
                response.IsSuccessful = true;
                response.NumberOfDocumentsRead = packages.Count;
                response.FileReadTime = TimeSpan.FromMilliseconds(fileReadTime.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
                logger.LogError($"Failed to process historical data file: Exception: {ex}");
            }
            return (response, packages, exceptions);
        }

        private PackageForArchive ParseRow(ExcelWorkSheet ws, int row,
            List<Site> sites, List <SubClient> subClients, Dictionary<string, List<Bin>> activeBinsBySite)
        {
            var packageId = ws.GetStringValue(row, "Package ID");
            if (StringHelper.DoesNotExist(packageId))
                return null;
            var location = ws.GetStringValue(row, "Location");
            if (StringHelper.DoesNotExist(location))
                return null;
            var parts = location.Replace(" ", "").Split('-');
            var clientName = string.Empty;
            var siteName = string.Empty;
            var subClientName = string.Empty;
            if (parts[0].Contains("DALC"))
            {
                var subClient = subClients.FirstOrDefault(s => s.ClientName == "DALC");
                if (subClient == null)
                    return null;
                clientName = subClient.ClientName;
                siteName = subClient.SiteName;
                subClientName = subClient.Name;
            }
            else
            {
                if (parts.Length < 2)
                    return null;
                clientName = parts[0];
                siteName = parts[1];
                var subClient = subClients.FirstOrDefault(s => s.SiteName == siteName && s.ClientName == clientName);
                if (subClient == null)
                    return null;
                subClientName = subClient.Name;
            }
            var site = sites.FirstOrDefault(s => s.SiteName == siteName) ?? new Site() { SiteName = siteName };

            var dateTime = ParseDateTime(ws.GetStringValue(row, "Manifest Date"), "12:00:00");
            var trackingNumber = ws.GetStringValue(row, "Tracking Number");
            Decimal.TryParse(ws.GetStringValue(row, "Weight"), out var weight);
            var shippingMethod = ws.GetStringValue(row, "Product");
            var shippingCarrier = ws.GetStringValue(row, "Carrier");

            var binCode = string.Empty;
            var binGroupId = string.Empty;
            if (activeBinsBySite.TryGetValue(siteName, out var activeBins))
            {
                var type = ws.GetStringValue(row, "Entry Unit Type");
                var zip = ws.GetFormattedIntValue(row, "Entry Unit Zip", 5);
                var name = ws.GetStringValue(row, "Entry Unit Name");
                if (StringHelper.Exists(type) && StringHelper.Exists(zip))
                {
                    var binsForType = activeBins.Where(b => b.BinCode.Length >= 1 && b.BinCode.Substring(0, 1) == type.Substring(0, 1)).ToList();
                    var bin = MatchBin(binsForType, type, zip);
                    if (bin == null && type == "NDC")
                    {
                        binsForType = activeBins.Where(b => b.BinCode.Length >= 1 && b.BinCode.Substring(0, 1) == "S" && 
                            b.DropShipSiteDescriptionPrimary == name).ToList();
                        bin = MatchBin(binsForType, "SCF", zip);
                    }
                    if (bin != null)
                    {
                        binCode = bin.BinCode;
                        binGroupId = bin.ActiveGroupId;
                    }
                }
            }
            var eventDateTime = ParseDateTime(ws.GetStringValue(row, "Last Known Status Date"), ws.GetStringValue(row, "Last Known Status Time"));
            var eventDescription = ws.GetStringValue(row, "Last Known Status Desc");
            var eventLocation = ws.GetStringValue(row, "Last Known Status Location");
            var package = new PackageForArchive()
            {
                PackageId = packageId,
                ClientName = clientName,
                SubClientName = subClientName,

                PackageStatus = EventConstants.Processed,
                LocalProcessedDate = dateTime,
                ShippedDate = dateTime,

                SiteName = siteName,

                ShippingMethod = shippingMethod,
                ShippingCarrier = shippingCarrier,
                ShippingBarcode = trackingNumber,
                Weight = weight,
                BinCode = binCode,

                StopTheClockEventDate = eventDateTime,
                LastKnownEventDate = eventDateTime,
                LastKnownEventDescription = eventDescription.ToUpperInvariant(),
                LastKnownEventLocation = eventLocation,

                PostalDays = postalDaysRepository.CalculatePostalDays(eventDateTime, dateTime, shippingMethod),
                CalendarDays = postalDaysRepository.CalculateCalendarDays(eventDateTime, dateTime),
                IsStopTheClock = 1,
                IsUndeliverable = 0,
            };
            return package;
        }

        private Bin MatchBin(List<Bin> binsForType, string type, string zip)
        {
            // First try to match whole zip and BinCode[0] == Type[0] ...
            var bin = binsForType.FirstOrDefault(b => b.LabelListZip == zip);
            // Then if Type = "SCF" match first 3 of zip and BinCode[0] == Type[0] ...
            if (bin == null && type == "SCF")
                bin = binsForType.FirstOrDefault(b => zip.Length >= 3 && b.LabelListZip == zip.Substring(0, 3));
            // Note: In caller b.DropShipSiteCszPrimary was replaced by CityStateZipUtility.ParseCityStateZip(b.DropShipSiteCszPrimary).FullZip.SubString(0,5) 
            // Try again with zip from DropShipSiteCszPrimary to match whole zip and BinCode[0] == Type[0] ...
            if (bin == null)
                bin = binsForType.FirstOrDefault(b => b.DropShipSiteCszPrimary == zip);
            // Then if Type = "SCF" match first 3 of zip and BinCode[0] == Type[0] ...
            if (bin == null && type == "SCF")
                bin = binsForType.FirstOrDefault(b => b.DropShipSiteCszPrimary.Length >= 3 && zip.Length >= 3 &&
                    b.DropShipSiteCszPrimary.Substring(0, 3) == zip.Substring(0, 3));
            return bin;
        }

        private static DateTime ParseDateTime(string dateString, string timeString)
        {
            var dateTime = DateTime.Now;
            if (StringHelper.Exists(dateString) && StringHelper.Exists(timeString))
            {
                DateTime.TryParseExact($"{FixupString(dateString, '/')} {FixupString(timeString, ':')}", "MM/dd/yyyy HH:mm:ss", null, DateTimeStyles.AssumeUniversal, out dateTime);
            }
            return dateTime;
         }

        private static string FixupString(string dateTimeString, char delim)
        {
            var parts = dateTimeString.Split(delim);
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length < 2)
                    parts[i] = "0" + parts[i];
            }
            return String.Join(delim, parts);
        }

    }
}