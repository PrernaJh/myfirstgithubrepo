using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using PackageTracker.Communications.Interfaces;
using PackageTracker.Communications.Models;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using PackageTracker.EodService.Data.Models;
using PackageTracker.EodService.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IEodPackageRepository = PackageTracker.EodService.Interfaces.IEodPackageRepository;

namespace PackageTracker.EodService.Processors
{
    public class PackageDetailProcessor : IPackageDetailProcessor
    {
        private readonly ILogger<PackageDetailProcessor> logger;
        private readonly IEmailConfiguration emailConfiguration;
        private readonly IEmailService emailService;
        private readonly IEodPackageRepository eodPackageRepository;

        public PackageDetailProcessor(ILogger<PackageDetailProcessor> logger,
            IEmailConfiguration emailConfiguration,
            IEmailService emailService,
            IEodPackageRepository eodPackageRepository
            )
        {
            this.logger = logger;
            this.emailConfiguration = emailConfiguration;
            this.emailService = emailService;
            this.eodPackageRepository = eodPackageRepository;
        }

        public async Task<FileExportResponse> GeneratePackageDetailFile(
            Site site, DateTime dateToProcess, string webJobId, bool isHistorical, bool addPackageRatingElements)
        {
            var response = new FileExportResponse();
            var totalWatch = Stopwatch.StartNew();
            var dbWriteWatch = new Stopwatch();

            var dbReadWatch = Stopwatch.StartNew();
            var eodPackageDetails = await eodPackageRepository.GetPackageDetails(site.SiteName, dateToProcess);
            dbReadWatch.Stop();

            if (eodPackageDetails.Any())
            {
                logger.LogInformation($"Processing {eodPackageDetails.Count()} Package Detail records for site: {site.SiteName}, date: {dateToProcess.Date}");

                //Add header record
                var HeaderStr = string.Empty;
                foreach (var property in typeof(PackageDetailRecord).GetProperties())
                {
                    if (property.Name != "Id" && property.Name != "CosmosId") // Exclude fields from EodChildRecord 
                    {
                        if (addPackageRatingElements)
                        {
                            HeaderStr += $"{property.Name}|";
                        }
                        else if (property.Name != "IsOutside48States" && 
                            property.Name != "IsRural") 
                        {
                            HeaderStr += $"{property.Name}|";
                        }
                    }
                }
                // To match old data to avoid problems with Qlik
                HeaderStr = HeaderStr.Replace("MarkupType", "MarkupReason");
                if (! addPackageRatingElements)
                {
                    HeaderStr += "Id|"; 
                }
                //
                response.FileContents.Add(HeaderStr + Environment.NewLine);

                foreach (var eodPackage in eodPackageDetails)
                {
                    response.FileContents.Add(BuildRecordString(eodPackage.PackageDetailRecord, addPackageRatingElements));
                    response.NumberOfRecords += 1;
                }
                if (!isHistorical)
                {
                    await SendSummaryEmail(site, eodPackageDetails.ToList());
                }
                response.IsSuccessful = true;
            }
            else
            {
                logger.LogInformation($"No Package Detail records found for for site: {site.SiteName}, date: {dateToProcess.Date}");
                response.IsSuccessful = true;
            }

            response.DbReadTime = TimeSpan.FromMilliseconds(dbReadWatch.ElapsedMilliseconds);
            response.DbWriteTime = TimeSpan.FromMilliseconds(dbWriteWatch.ElapsedMilliseconds);
            response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);

            return response;
        }

        public PackageDetailRecord CreatePackageDetailRecord(Package package)
        {
            var shipAndBillMethod = GetPackageShippingMethod(package);
            var record = new PackageDetailRecord
            {
                CosmosId = package.Id,
                MmsLocation = package.SiteName,
                Customer = package.ClientName,
                ShipDate = package.LocalProcessedDate.ToString("MM/dd/yyyy"),
                VamcId = PackageIdUtility.GetPaddedVisnSiteParentId(package.ClientName, package.PackageId, 5),
                PackageId = package.PackageId,
                TrackingNumber = package.Barcode ?? string.Empty,
                ShipMethod = shipAndBillMethod ?? string.Empty,
                BillMethod = shipAndBillMethod ?? string.Empty,
                EntryUnitType = GetEntryUnitType(package),
                ShipCost = package.Cost.ToString(),
                BillingCost = package.Charge.ToString(),
                SignatureCost = package.ServiceLevel == ServiceLevelConstants.Signature ? package.Cost.ToString() : string.Empty,
                ShipZone = package.Zone.ToString(),
                ZipCode = package.Zip ?? string.Empty,
                Weight = package.Weight.ToString() ?? string.Empty,
                BillingWeight = package.BillingWeight.ToString(),
                SortCode = package.BinCode ?? string.Empty,
                IsOutside48States = package.IsOutside48States ? "Y" : "N",
                IsRural = package.IsRural ? "Y" : "N",
                MarkupType = package.MarkUpType,
            };

            return record;
        }

        private async Task SendSummaryEmail(Site site, List<EodPackage> packageDetailRecords)
        {
            var email = new EmailMessage();
            site.EodSummaryEmailList.Where(x => StringHelper.Exists(x)).ToList()
                .ForEach(x => email.ToAddresses.Add(new EmailAddress { Name = x, Address = x }));
            if (!email.ToAddresses.Any())
                return;

            // Send summary email:
            var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
            var fileName = $"{siteLocalTime.ToString("yyyyMMdd_HHmmss")}_{site.SiteName}_PackageDetailSummary.xlsx";
            var ws = new ExcelWorkSheet($"{site.SiteName} Summary",
                new string[] { "Customer", "Product", "Pieces", "Weight", "Barcodes" });
            var dataTypes = new eDataTypes[] { eDataTypes.String, eDataTypes.String, eDataTypes.Number, eDataTypes.Number, eDataTypes.Number };

            foreach (var customer in packageDetailRecords.GroupBy(p => p.PackageDetailRecord.Customer).OrderBy(g => g.Key))
            {
                foreach (var product in customer.GroupBy(p => p.PackageDetailRecord.ShipMethod).OrderBy(g => g.Key))
                {
                    var count = 0;
                    decimal sum = 0;
                    foreach (var package in product)
                    {
                        count++;
                        decimal.TryParse(package.PackageDetailRecord.Weight, out var weight);
                        sum += weight;
                    }
                    var uniqueBarcodes = product.Select(p => p.PackageDetailRecord).GroupBy(pd => pd.TrackingNumber).Count();
                    ws.InsertRow(ws.RowCount + 1,
                        new string[] { customer.Key, product.Key, count.ToString(), sum.ToString(".00"), uniqueBarcodes.ToString() }, dataTypes);
                }
            }
            var totalRow = ws.RowCount + 1;
            ws.InsertRow(totalRow, new string[] { "Totals", string.Empty, string.Empty, string.Empty }, dataTypes);
            ws.InsertFormula($"C{totalRow}", $"SUM(C2:C{totalRow - 1})");
            ws.InsertFormula($"D{totalRow}", $"SUM(D2:D{totalRow - 1})");
            ws.InsertFormula($"E{totalRow}", $"SUM(E2:E{totalRow - 1})");
            email.FromAddresses.Add(new EmailAddress { Name = emailConfiguration.SmtpUsername, Address = emailConfiguration.SmtpUsername });
            email.Subject = $"EOD Package Summary Report for {site.SiteName} {siteLocalTime.ToString("g")}";
            email.Content = "See Attachment.";
            var attachments = new List<EmailAttachment>();
            attachments.Add(new EmailAttachment
            {
                MimeType = MimeTypeConstants.OPEN_OFFICE_SPREADSHEET,
                FileName = fileName,
                FileContents = await ws.GetContentsAsync()
            });
            await emailService.SendAsync(email, false, attachments);
        }

        private string GetEntryUnitType(Package package)
        {
            var response = string.Empty;
            if (StringHelper.Exists(package.BinCode))
            {
                if (package.BinCode.Substring(0, 1).ToUpper() == "S")
                {
                    response = "SCF";
                }
                else if (package.BinCode.Substring(0, 1).ToUpper() == "D")
                {
                    response = "DDU";
                }
            }
            return response;
        }

        private string GetPackageShippingMethod(Package package)
        {
            var response = string.Empty;
            if (package.ShippingCarrier == ShippingCarrierConstants.Usps)
            {
                response = package.ShippingMethod switch
                {
                    ShippingMethodConstants.UspsParcelSelectLightWeight => "IRREGULAR",
                    ShippingMethodConstants.UspsParcelSelect => "IRREGULAR",
                    ShippingMethodConstants.UspsFirstClass => "FIRST CLASS",
                    ShippingMethodConstants.UspsFcz => "FIRST CLASS",
                    ShippingMethodConstants.UspsPriority => "PRIORITY MAIL",
                    ShippingMethodConstants.UspsPriorityExpress => "PRIORITY EXPRESS",
                    ShippingMethodConstants.UspsPmod => "PRIORITY MAIL (PMOD)",
                    _ => string.Empty
                };
            }
            else if (package.ShippingCarrier == ShippingCarrierConstants.Ups)
            {
                response = package.ShippingMethod switch
                {
                    ShippingMethodConstants.UpsGround => "UPS - GROUND",
                    ShippingMethodConstants.UpsNextDayAir => "UPS - NEXT DAY AIR",
                    ShippingMethodConstants.UpsNextDayAirSaver => "UPS - NEXT DAY AIR SAVER",
                    ShippingMethodConstants.UpsSecondDayAir => "UPS - 2ND DAY AIR",
                    _ => string.Empty
                };
            }
            else if (package.ShippingCarrier == ShippingCarrierConstants.FedEx)
            {
                response = package.ShippingMethod switch
                {
                    ShippingMethodConstants.FedExGround => "FEDEX - GROUND",
                    ShippingMethodConstants.FedExPriorityOvernight => "FEDEX - EXPRESS",
                    _ => string.Empty
                };
            }

            return response;
        }

        public static string BuildRecordString(PackageDetailRecord record, bool addPackageRatingElements)
        {
            var delimiter = "|";
            var recordBuilder = new StringBuilder();

            recordBuilder.Append(record.MmsLocation);
            recordBuilder.Append(delimiter + record.Customer);
            recordBuilder.Append(delimiter + record.ShipDate);
            recordBuilder.Append(delimiter + record.VamcId);
            recordBuilder.Append(delimiter + record.PackageId);
            recordBuilder.Append(delimiter + record.TrackingNumber);
            recordBuilder.Append(delimiter + record.ShipMethod);
            recordBuilder.Append(delimiter + record.BillMethod);
            recordBuilder.Append(delimiter + record.EntryUnitType);
            recordBuilder.Append(delimiter + record.ShipCost);
            recordBuilder.Append(delimiter + record.BillingCost);
            recordBuilder.Append(delimiter + record.SignatureCost);
            recordBuilder.Append(delimiter + record.ShipZone);
            recordBuilder.Append(delimiter + record.ZipCode);
            recordBuilder.Append(delimiter + record.Weight);
            recordBuilder.Append(delimiter + record.BillingWeight);
            recordBuilder.Append(delimiter + record.SortCode);
            if (addPackageRatingElements)
            {
                recordBuilder.Append(delimiter + record.MarkupType);
                recordBuilder.Append(delimiter + record.IsOutside48States);
                recordBuilder.Append(delimiter + record.IsRural);
            }
            recordBuilder.Append(delimiter); // To match old data to avoid problems with Qlik
            recordBuilder.AppendLine();

            return recordBuilder.ToString();
        }
    }
}