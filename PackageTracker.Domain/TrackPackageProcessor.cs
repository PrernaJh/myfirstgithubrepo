using FedExTrackApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Data.Models.TrackingData;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Models.TrackPackages.Ups;
using PackageTracker.Domain.Models.TrackPackages.Usps;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PackageTracker.Domain
{
    public class TrackPackageProcessor : ITrackPackageProcessor
    {
        private readonly ILogger<TrackPackageProcessor> logger;

        private readonly IBlobHelper blobHelper;
        private readonly IConfiguration config;
        private readonly HttpClient httpClient;
        private readonly ISiteProcessor siteProcessor;
        private readonly ISubClientProcessor subClientProcessor;

        public TrackPackageProcessor(ILogger<TrackPackageProcessor> logger,
            IBlobHelper blobHelper,
            IConfiguration config,
            HttpClient httpClient, 
            ISiteProcessor siteProcessor,
            ISubClientProcessor subClientProcessor)
        {
            this.logger = logger;
            this.blobHelper = blobHelper;
            this.config = config;
            this.httpClient = httpClient;
            this.siteProcessor = siteProcessor;
            this.subClientProcessor = subClientProcessor;
        }

        public void ParseUpsTrackingDataAsync(bool logTrackPackages, List<TrackPackage> trackPackages, UpsTrackPackageResponse trackPackageResponse)
        {
            if (trackPackageResponse.QuantumViewEvents?.SubscriptionEvents != null)
            {
                foreach (var subscriptionEvent in trackPackageResponse.QuantumViewEvents.SubscriptionEvents)
                {
                    foreach (var subscriptionFile in subscriptionEvent.SubscriptionFile)
                    {
                        foreach (var item in subscriptionFile.Delivery)
                        {
                            DateTime.TryParseExact($"{item.Date} {item.Time}", "yyyyMMdd HHmmss", null, DateTimeStyles.AssumeUniversal, out var deliveryDateTime);
                            var trackingData = new UpsTrackingData
                            {
                                ShipperNumber = item.ShipperNumber,
                                ActivityLocationPoliticalDivision2 = item.ActivityLocation.AddressArtifactFormat.PoliticalDivision2,
                                ActivityLocationPoliticalDivision1 = item.ActivityLocation.AddressArtifactFormat.PoliticalDivision1,
                                ActivityLocationCountryCode = item.ActivityLocation.AddressArtifactFormat.CountryCode,
                                ActivityLocationPostcodePrimaryLow = item.ActivityLocation.AddressArtifactFormat.PostcodePrimaryLow,
                                DeliveryDateTime = deliveryDateTime,
                                DeliveryLocationStreetNumberLow = item.DeliveryLocation.AddressArtifactFormat.StreetNumberLow,
                                DeliveryLocationStreetName = item.DeliveryLocation.AddressArtifactFormat.StreetName,
                                DeliveryLocationStreetType = item.DeliveryLocation.AddressArtifactFormat.StreetType,
                                DeliveryLocationPoliticalDivision2 = item.DeliveryLocation.AddressArtifactFormat.PoliticalDivision2,
                                DeliveryLocationPoliticalDivision1 = item.DeliveryLocation.AddressArtifactFormat.PoliticalDivision1,
                                DeliveryLocationCountryCode = item.DeliveryLocation.AddressArtifactFormat.CountryCode,
                                DeliveryLocationPostcodePrimaryLow = item.DeliveryLocation.AddressArtifactFormat.PostcodePrimaryLow,
                                DeliveryLocationResidentialAddressIndicator = item.DeliveryLocation.AddressArtifactFormat.ResidentialAddressIndicator,
                                DeliveryLocationCode = item.DeliveryLocation.Code,
                                DeliveryLocationDescription = item.DeliveryLocation.Description,
                                DeliveryLocationSignedForByName = item.DeliveryLocation.SignedForByName
                            };
                            var trackPackage = new TrackPackage
                            {
                                CreateDate = DateTime.Now,
                                TrackingNumber = item.TrackingNumber,
                                PartitionKey = PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(item.TrackingNumber),
                                ShippingCarrier = ShippingCarrierConstants.Ups,
                                UpsTrackingData = trackingData
                            };
                            trackPackages.Add(trackPackage);
                            if (logTrackPackages)
                                logger.LogInformation(JsonUtility<TrackPackage>.Serialize(trackPackage));
                        }
                    }
                }
            }

        }

        public async Task<(XmlImportResponse, List<TrackPackage>)> ImportUpsTrackingDataAsync(WebJobSettings webJobSettings)
        {
            var response = new XmlImportResponse();
            var trackPackages = new List<TrackPackage>();
            string bookmarkRequest = null;
            while (!response.IsCompleted)
            {
                try
                {
                    var queryWatch = new Stopwatch();
                    logger.Log(LogLevel.Information, "Beginning UPS API call");
                    var trackPackageResponse = await GetUpsPackages(bookmarkRequest);
                    logger.Log(LogLevel.Information, "UPS API call complete");

                    if (trackPackageResponse.Bookmark != null)
                    {
                        response.Bookmark = trackPackageResponse.Bookmark;
                    }
                    else
                    {
                        response.IsCompleted = true;
                    }
                    if (trackPackageResponse.Response?.Error != null && trackPackageResponse.Response.Error.ErrorCode != 0)
                    {
                        var error = trackPackageResponse.Response.Error;
                        if (error.ErrorCode != 330028) // There is no unread file for subscription ...
                        {
                            logger.LogError($"UPS track packages request returned error: Code: {error.ErrorCode}, Description: {error.ErrorDescription}");
                            response.IsSuccessful = false;
                            break;
                        }
                    }
                    ParseUpsTrackingDataAsync(webJobSettings.GetParameterBoolValue("LogTrackPackages"), trackPackages, trackPackageResponse);
                    response.NumberOfDocumentsImported = trackPackages.Count;
                    response.IsSuccessful = true;
                }
                catch (Exception ex)
                {
                    logger.LogError($"Exception in Ups track package processor: {ex}");
                    response.Message = ex.Message;
                    response.IsSuccessful = false;
                    break;
                }
            }
            return (response, trackPackages);
        }

        private async Task<UpsTrackPackageResponse> GetUpsPackages(string bookmarkRequest)
        {
            var trackPackageResponse = new UpsTrackPackageResponse();
            var requestUri = config.GetSection("UpsApis").GetSection("TrackPackageEndpointUri").Value;
            var (accessRequest, trackpackageRequest) = GenerateUpsRequestModel(bookmarkRequest);
            var archivePath = config.GetSection("UpsScanDataArchive").Value;

            var requestXml = XmlUtility<UpsAccessRequest>.Serialize(accessRequest) +
                XmlUtility<UpsTrackPackageRequest>.Serialize(trackpackageRequest);
            using (var xmlContent = new StringContent(requestXml))
            {
                var httpClientResponse = await httpClient.PostAsync(requestUri, xmlContent);
                if (httpClientResponse.IsSuccessStatusCode)
                {
                    var responseContent = await httpClientResponse.Content.ReadAsStringAsync();
                    var serializer = new XmlSerializer(typeof(UpsTrackPackageResponse));

                    using (var reader = new StringReader(responseContent))
                    {
                        trackPackageResponse = (UpsTrackPackageResponse)serializer.Deserialize(reader);
                    }
                    int eventCount = 0;
                    if (trackPackageResponse.QuantumViewEvents?.SubscriptionEvents != null)
                    {
                        foreach (var subscriptionEvent in trackPackageResponse.QuantumViewEvents.SubscriptionEvents)
                        {
                            foreach (var SubscriptionFile in subscriptionEvent.SubscriptionFile)
                            {
                                eventCount += SubscriptionFile.Delivery.Count;
                            }
                        }
                    }
                    if (eventCount > 0 && StringHelper.Exists(archivePath))
                    {
                        var filename = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}[{eventCount}].xml";
                        logger.LogInformation($"GetUpsPackages: Event Count: {eventCount}, fileName: {archivePath}/{filename}");
                        await blobHelper.UploadListOfStringsToBlobAsync(
                            new List<string> { XmlUtility<UpsTrackPackageResponse>.Serialize(trackPackageResponse) }, 
                            archivePath, filename);
                    }
                }
            };
            return trackPackageResponse;
        }

        private (UpsAccessRequest accessRequest, UpsTrackPackageRequest trackpackageRequest) GenerateUpsRequestModel(string bookmarkRequest)
        {
            var accessLicenseNumber = config.GetSection("UpsApis").GetSection("AccessLicenseNumber").Value;
            var username = config.GetSection("UpsApis").GetSection("Username").Value;
            var password = config.GetSection("UpsApis").GetSection("Password").Value;
            var subscription = config.GetSection("UpsApis").GetSection("QuantumEventsSubscription").Value;
            var bookmarkData = string.Empty;

            var accessRequest = new UpsAccessRequest
            {
                AccessLicenseNumber = accessLicenseNumber,
                UserId = username,
                Password = password
            };

            if (!string.IsNullOrWhiteSpace(bookmarkRequest))
            {
                bookmarkData = bookmarkRequest;
            }
            var trackPackageRequest = new UpsTrackPackageRequest
            {
                Request = new Request
                {
                    RequestAction = "QVEvents"

                },
                SubscriptionRequest = new SubscriptionRequest
                {
                    Name = subscription
                },               
                Bookmark = bookmarkData,
            };

            return (accessRequest, trackPackageRequest);
        }

        public async Task<UspsTrackPackageResponse> GetUspsTrackingData(string trackingID, string userId, string sourceId)
        {
            UspsTrackPackageResponse trackPackageResponse = new UspsTrackPackageResponse();
            var trackPackageRequest = await GenerateUspsTrackPackageRequestModel(trackingID, userId, sourceId);
            var requestUri = config.GetSection("UspsApis").GetSection("TrackPackageUriRoot").Value;

            var requestXml = XmlUtility<UspsTrackPackageRequest>.SerializeOmitScheme(trackPackageRequest);
            //var requestXml = @"<TrackFieldRequest USERID=""535TECMA0117""><Revision>1</Revision><ClientIp>127.0.0.0</ClientIp><SourceId>TECMailing</SourceId><TrackID ID=""4203832993748202113000000028928939"" /></TrackFieldRequest>";
            requestUri += requestXml;

            using (var xmlContent = new StringContent(requestXml))
            {
                var httpClientResponse = await httpClient.PostAsync(requestUri, null);
                if (httpClientResponse.IsSuccessStatusCode)
                {
                    var responseContent = await httpClientResponse.Content.ReadAsStringAsync();
                    var serializer = new XmlSerializer(typeof(UspsTrackPackageResponse));

                    using (var reader = new StringReader(responseContent))
                    {
                        trackPackageResponse = (UspsTrackPackageResponse)serializer.Deserialize(reader);
                    }
                }
                else
                {
                    logger.LogError($"TrackFieldRequest Failed: status code: {httpClientResponse.StatusCode}, reason: {httpClientResponse.ReasonPhrase}");
                }
            };

            return trackPackageResponse;
        }

        private async Task<UspsTrackPackageRequest> GenerateUspsTrackPackageRequestModel(string trackingID, string userId, string sourceId)
        {
            var request = new UspsTrackPackageRequest
            {
                ClientIp = config.GetSection("UspsApis").GetSection("ClientIp").Value,
                Revision = config.GetSection("UspsApis").GetSection("Revision").Value,
                USERID = userId,
                SourceId = sourceId,
                TrackID = new List<TrackID>
                {
                    new TrackID
                    {
                        ID = trackingID
                    }
                }

            };
            await Task.CompletedTask;
            return request;
        }

        public List<TrackPackage> ImportUspsTrackPackageResponse(UspsTrackPackageResponse response, string shippingBarcode)
        {
            var trackPackages = new List<TrackPackage>();
            if (response != null && response.TrackInfo != null)
            {
                var trackInfo = response.TrackInfo.Find(ti => ti.ID == shippingBarcode);
                if (trackInfo != null)
                {
                    if (trackInfo.TrackDetail != null)
                    {
                        foreach (var trackDetail in trackInfo.TrackDetail)
                        {
                            if (trackDetail.EventCode != "MA")
                            {
                                var item = new TrackPackage
                                {
                                    CreateDate = DateTime.Now,
                                    ShippingCarrier = ShippingCarrierConstants.Usps,
                                };
                                item.UspsTrackingData = new UspsTrackingData();
                                item.TrackingNumber = shippingBarcode;
                                item.PartitionKey = PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(item.TrackingNumber);
                                item.UspsTrackingData.ScanningFacilityZip = trackDetail.EventZIPCode;
                                item.UspsTrackingData.EventCode = trackDetail.EventCode;
                                item.UspsTrackingData.EventName = trackDetail.Event.ToUpper();
                                item.UspsTrackingData.EventDateTime = trackDetail.EventDateTime();
                                item.UspsTrackingData.ScanningFacilityName = trackDetail.EventCity + (trackDetail.EventState.Trim().Length > 0 ? ", " + trackDetail.EventState : "");

                                trackPackages.Add(item);
                            }
                        }
                    }
                    if (trackInfo.TrackSummary != null)
                    {
                        if (trackInfo.TrackSummary.EventCode != "MA")
                        {
                            var item = new TrackPackage
                            {
                                CreateDate = DateTime.Now,
                                ShippingCarrier = ShippingCarrierConstants.Usps,
                            };
                            item.UspsTrackingData = new UspsTrackingData();
                            item.TrackingNumber = shippingBarcode;
                            item.PartitionKey = PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(item.TrackingNumber);
                            item.UspsTrackingData.ScanningFacilityZip = trackInfo.TrackSummary.EventZIPCode;
                            item.UspsTrackingData.EventCode = trackInfo.TrackSummary.EventCode;
                            item.UspsTrackingData.EventName = trackInfo.TrackSummary.Event.ToUpper();
                            item.UspsTrackingData.EventDateTime = trackInfo.TrackSummary.EventDateTime();
                            item.UspsTrackingData.ScanningFacilityName = trackInfo.TrackSummary.EventCity + (trackInfo.TrackSummary.EventState.Trim().Length > 0 ? ", " + trackInfo.TrackSummary.EventState : "");

                            trackPackages.Add(item);
                        }
                    }
                }
            }
            return trackPackages;
        }

        public async Task<(FileReadResponse, List<TrackPackage>)> ImportUspsTrackingFileAsync(WebJobSettings webJobSettings, Stream stream, int chunk)
        {
            var response = new FileReadResponse();
            var trackPackages = new List<TrackPackage>();
            try
            {
                var fileReadTime = new Stopwatch();

                fileReadTime.Start();
                trackPackages = await ReadUspsTrackingFileStreamAsync(webJobSettings.GetParameterBoolValue("LogTrackPackages"), stream, chunk);
                response.NumberOfDocumentsRead = trackPackages.Count;
                response.IsSuccessful = true;
                fileReadTime.Stop();

                logger.Log(LogLevel.Information,$"USPS Tracking File stream rows read: {response.NumberOfDocumentsRead}");

                response.FileReadTime = TimeSpan.FromMilliseconds(fileReadTime.ElapsedMilliseconds);
            } 
            catch(Exception ex)
            {
                response.Message = ex.Message;
            }
            return (response, trackPackages);
        }

        private static string Extract(string line, string columns)
        {
            var parts = Regex.Split(columns, @"\D+");
            if (parts.Length > 1 && 
                int.TryParse(parts[0], out var start) &&
                int.TryParse(parts[1], out var end))
            {
                return line.Substring(start - 1, end - start + 1).Trim();
            }
            return string.Empty;
        }

        private static TrackPackage ParseUspsTrackingData(string line, List<Site> sites)
        {
            if (StringHelper.Exists(line) && line.Length > 392)
            {
                try
                {
                    var eventCode = Extract(line, "174-175");                 // Event Code
                    if (eventCode == "MA")  // EventCode = "MA", EventDescription = "PRE-SHIPMENT INFO SENT USPS AWAITS ITEM"
                        return null;        // This isn't scan data
                    var item = new TrackPackage { 
                        CreateDate = DateTime.Now,
                        ShippingCarrier = ShippingCarrierConstants.Usps,
                    };
                    item.UspsTrackingData = new UspsTrackingData();
                    var fileVersion = Extract(line, "002-004");                                 // USPS Event Extract File Version e.g. 1.5 == "015"
                    item.TrackingNumber = Extract(line, "008-041");                             // Tracking Number
                    item.PartitionKey = PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(item.TrackingNumber);
                    item.UspsTrackingData.ElectronicFileNumber = Extract(line, "045-078");      // Electronic File Number 
                    item.UspsTrackingData.MailerID = Extract(line, "082-090");                  // Mailer ID
                    item.UspsTrackingData.MailerName = Extract(line, "094-113");                // Mailer Name
                    item.UspsTrackingData.DestinationZipCode = Extract(line, "117-121");        // Destination ZIP Code
                    item.UspsTrackingData.DestinationZipPlus4 = Extract(line, "125-128");       // Destination ZIP+4
                    item.UspsTrackingData.ScanningFacilityZip = Extract(line, "132-136");       // Scanning Facility ZIP
                    item.UspsTrackingData.ScanningFacilityName = Extract(line, "140-170");      // Scanning Facility Name
                    item.UspsTrackingData.EventCode = Extract(line, "174-175");                 // Event Code
                    item.UspsTrackingData.EventName = Extract(line, "179-218");                 // Event Name
                    string eventDate = Extract(line, "222-229");                                        //  Event Date 
                    string eventTime = Extract(line, "233-236");                                        //  Event Time
                    DateTime.TryParseExact($"{eventDate} {eventTime}", "yyyyMMdd HHmm", null, DateTimeStyles.AssumeUniversal, out var eventDateTime);
                    item.UspsTrackingData.EventDateTime = eventDateTime;
                    item.UspsTrackingData.MailOwnerMailerID = Extract(line, "240-248");         // Mail Owner Mailer ID 
                    item.UspsTrackingData.CustomerReferenceNumber = Extract(line, "252-281");   // Customer Reference Number
                    item.UspsTrackingData.DestinationCountryCode = Extract(line, "285-286");    // Destination Country Code
                    item.UspsTrackingData.RecipientName = Extract(line, "290-309");             // Recipient Name
                    item.UspsTrackingData.OriginalLabel = Extract(line, "313-346");             // Original Label 
                    item.UspsTrackingData.UnitOfMeasureCode = Extract(line, "350-350");         // Unit of Measure Code
                    item.UspsTrackingData.Weight = Extract(line, "354-362");                    // Weight 
                    string deliveryDate = Extract(line, "366-373");                                     // Guaranteed Delivery Date
                    string deliveryTime = Extract(line, "377-380");                                     // Guaranteed Delivery Time
                    DateTime.TryParseExact($"{deliveryDate} {deliveryTime}", "yyyyMMdd HHmm", null, DateTimeStyles.AssumeUniversal, out var deliveryDateTime);
                    item.UspsTrackingData.DeliveryDateTime = deliveryDateTime;
                    item.UspsTrackingData.LogisticsManagerMailerID = Extract(line, "384-392");  // Logistics Manager Mailer ID
#if false // Need to find something else that identifies the site
                    var site = sites.FirstOrDefault(s => s.MailProducerMid == item.UspsTrackingData.MailerID);
                    item.SiteName = site?.SiteName;
#endif
                    return item;
                }
                catch
                {
                }
            }
            return null;
        }

        public async Task<List<TrackPackage>> ReadUspsTrackingFileStreamAsync(bool logTrackPackages, Stream stream, int chunk)
        {
           var trackPackages = new List<TrackPackage>();
           var sites = (await siteProcessor.GetAllSitesAsync()).ToList();
           try
           {
                var reader = new StreamReader(stream);
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var trackPackage = ParseUspsTrackingData(line, sites);
                    if (trackPackage != null)
                    {
                        trackPackages.Add(trackPackage);
                        if (logTrackPackages)
                            logger.LogInformation(JsonUtility<TrackPackage>.Serialize(trackPackage));
                    }
                    if (chunk != 0 && trackPackages.Count == chunk)
                        break;
                }
                return trackPackages;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error,$"Failed to read USPS Tracking file. Exception: { ex }");
                return trackPackages;
            }
        }

        public async Task<(FileReadResponse, List<TrackPackage>)> ImportFedExTrackingFileAsync(WebJobSettings webJobSettings, Stream stream)
        {
            var response = new FileReadResponse();
            var trackPackages = new List<TrackPackage>();
            try
            {
                var fileReadTime = new Stopwatch();

                fileReadTime.Start();
                trackPackages = await ReadFedExTrackingFileStreamAsync(webJobSettings.GetParameterBoolValue("LogTrackPackages"), stream);
                response.NumberOfDocumentsRead = trackPackages.Count;
                response.IsSuccessful = true;
                fileReadTime.Stop();

                logger.Log(LogLevel.Information, $"FedEx Tracking File stream rows read: {response.NumberOfDocumentsRead}");

                response.FileReadTime = TimeSpan.FromMilliseconds(fileReadTime.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return (response, trackPackages);
        }

        private static TrackPackage ParseFedExTrackingData(string line)
        {
            if (StringHelper.Exists(line.Trim()))
            {
                try
                {
                    var item = new TrackPackage
                    {
                        CreateDate = DateTime.Now,
                        ShippingCarrier = ShippingCarrierConstants.FedEx,
                    };
                    var parts = line.Trim().Split(',');
                    item.TrackingNumber = parts[3].Trim();                            // Tracking Number
                    if (item.TrackingNumber == "TRCK#")
                        return null; // skip header record.
                    item.FedExTrackingData = new FedExTrackingData();
                    item.PartitionKey = PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(item.TrackingNumber);
                    var statusCode = parts[59].Trim();
                    item.FedExTrackingData.LastStatusCode = statusCode;
                    item.FedExTrackingData.LastStatusDescription = parts[72];
                    var city = parts[67].Trim();                                   // Event City
                    var state = parts[68].Trim();                                  // Event State
                    item.FedExTrackingData.EventAddress = StringHelper.Exists(state) ? $"{city}, {state}" : city;
                    var eventDate =  parts[15].Trim();                             //  Last Status Date 
                    var eventTime =  parts[16].Trim();                             //  Last Status Time
                    if(DateTime.TryParseExact($"{eventDate} {eventTime}", "yyyyMMdd HHmm", 
                            null, DateTimeStyles.AssumeUniversal, out var eventDateTime))
                        item.FedExTrackingData.LastStatusDateTime = eventDateTime;
                    return item;
                }
                catch
                {
                }
            }
            return null;
        }

        public async Task<List<TrackPackage>> ReadFedExTrackingFileStreamAsync(bool logTrackPackages, Stream stream)
        {
            var trackPackages = new List<TrackPackage>();
            try
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        var trackPackage = ParseFedExTrackingData(line);
                        if (trackPackage != null)
                        {
                            trackPackages.Add(trackPackage);
                            if (logTrackPackages)
                                logger.LogInformation(JsonUtility<TrackPackage>.Serialize(trackPackage));
                        }
                    }
                }
                return trackPackages;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to read FedEx Tracking file. Exception: { ex }");
                return trackPackages;
            }
        }

#if false // archaic
        public async Task<TrackingDataImportResponse> ImportFedexTrackingDataAsync(Site site)
        {
            var response = new TrackingDataImportResponse();
            try
            {
                var trackPackages = new List<TrackPackage>();
                var queryWatch = new Stopwatch();
                var packages = await packagePostProcessor.GetPackagesByShippingCarrierAsync(site.SiteName, ShippingCarrierConstants.FedEx);
                // Need to poll FedEx for each shipped package.
                foreach (var package in packages.Where(p => StringHelper.Exists(p.Id)))
                {
                    var allEvents = await GetTrackPackagesAsync(package.Barcode);
                    var events = await GetFedExTrackingDataAsync(package);
                    foreach (var trackEvent in events)
                    {
                        // Only store new events.
                        if (allEvents.FirstOrDefault(e => e.FedExTrackingData?.EventDateTime == trackEvent.EventDateTime) == null)
                        {
                            trackPackages.Add(new TrackPackage
                            {
                                CreateDate = DateTime.Now,
                                TrackingNumber = package.Barcode,
                                PartitionKey = PartitionKeyUtility.GenerateConstantLengthPartitionKeyString(package.Barcode),
                                ShippingCarrier = ShippingCarrierConstants.FedEx,
                                FedExTrackingData = trackEvent
                            });
                        }
                    }
                }
                if (trackPackages.Any())
                {
                    var serializedTrackPackages = JsonUtility<TrackPackage>.SerializeList(trackPackages);

                    var bulkResponse = await bulkProcessor.BulkImportDocumentsToDbAsync(serializedTrackPackages, CollectionNameConstants.TrackPackages);
                    response.DbInsertTime = TimeSpan.FromMilliseconds(queryWatch.ElapsedMilliseconds);

                    if (bulkResponse.BadInputDocuments.Any())
                    {
                        response.BadInputDocuments.AddRange(bulkResponse.BadInputDocuments);
                    }
                    response.NumberOfDocumentsImported += bulkResponse.NumberOfDocumentsImported;
                    response.RequestUnitsConsumed += bulkResponse.TotalRequestUnitsConsumed;
                }
                response.IsCompleted = true;
                response.IsSuccessful = true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception in FedEx track package processor: {ex}");
                response.IsSuccessful = false;
            }
            return response;
        }

        private async Task<IEnumerable<FedExTrackingData>> GetFedExTrackingDataAsync(Data.Models.Package package)
        {
            logger.Log(LogLevel.Information, "Beginning FedEx API call");
            var trackPackagesResponse = await GetFedExRawTrackingDataAsync(package);

            logger.Log(LogLevel.Information, "FedEx API call complete");
            var trackPackages = new List<FedExTrackingData>();
            if (trackPackagesResponse.TrackReply?.CompletedTrackDetails != null &&
                trackPackagesResponse.TrackReply.CompletedTrackDetails.Length > 0 &&
                trackPackagesResponse.TrackReply.CompletedTrackDetails[0].TrackDetails != null &&
                trackPackagesResponse.TrackReply.CompletedTrackDetails[0].TrackDetails.Length > 0 &&
                trackPackagesResponse.TrackReply.CompletedTrackDetails[0].TrackDetails[0].Events != null
                )
            {
                var events = trackPackagesResponse.TrackReply.CompletedTrackDetails[0].TrackDetails[0].Events;
                foreach (var trackEvent in events)
                {
                    if (trackEvent.Timestamp != null)
                    {
                        var item = new FedExTrackingData();
                        item.EventDateTime = trackEvent.Timestamp;
                        item.EventType = trackEvent.EventType ?? string.Empty;
                        item.EventDescription = trackEvent.EventDescription ?? string.Empty;
                        item.EventLocation = trackEvent.ArrivalLocation.ToString();
                        if (trackEvent.Address != null)
                        {
                            item.EventAddress = $"{trackEvent.Address.City}, {trackEvent.Address.StateOrProvinceCode} {trackEvent.Address.PostalCode}";
                        }
                        trackPackages.Add(item);
                    }
                }   
            }
            return trackPackages;
        }

        private async Task<trackResponse> GetFedExRawTrackingDataAsync(Data.Models.Package package)
        {
            /* Test Tracking numbers ...
               449044304137821 = Shipment information sent to FedEx
               149331877648230 = Tendered
               020207021381215 = Picked Up
               403934084723025 = Arrived at FedEx location
               920241085725456 = At local FedEx facility
               568838414941 = At destination sort facility
               039813852990618 = Departed FedEx location
               231300687629630 = On FedEx vehicle for delivery
               797806677146 = International shipment release
               377101283611590 = Customer not available or business closed
               852426136339213 = Local Delivery Restriction
               797615467620 = Incorrect Address
               957794015041323 = Unable to Deliver
               076288115212522 = Returned to Sender/Shipper
               581190049992 = International Clearance delay
               122816215025810 = Delivered
               843119172384577 = Hold at Location
               070358180009382 = Shipment Canceled
             */

            var subClient = await subClientProcessor.GetSubClientByNameAsync(package.SubClientName);
            var accountNumber = subClient.FedexAccountNumber;

            var request = new TrackRequest
            {
                WebAuthenticationDetail = new WebAuthenticationDetail
                {
                    UserCredential = new WebAuthenticationCredential
                    {
                        Key = config["FedExApis:ApiKey"],
                        Password = config["FedExApis:ApiPassword"]
                    }
                },
                Version = new VersionId
                {
                    ServiceId = "trck",
                    Major = 19,
                    Intermediate = 0,
                    Minor = 0
                },
                ClientDetail = new ClientDetail
                {
                    MeterNumber = config["FedExApis:MeterNumber"],
                    AccountNumber = accountNumber,
                },
                SelectionDetails = new TrackSelectionDetail[]
                {
                    new TrackSelectionDetail
                    {
                        PackageIdentifier = new TrackPackageIdentifier
                        {
                            Value = package.Barcode,
                            Type = TrackIdentifierType.TRACKING_NUMBER_OR_DOORTAG
                        },
                        ShipDateRangeBeginSpecified = false,
                        ShipDateRangeEndSpecified = false
                    }
                },
                ProcessingOptions = new TrackRequestProcessingOptionType[]
                {
                    TrackRequestProcessingOptionType.INCLUDE_DETAILED_SCANS
                }
            };
#if DEBUG
            logger.LogInformation(XmlUtility<TrackRequest>.Serialize(request));
#endif
            var response = await fedExTrackClient.trackAsync(request);
#if DEBUG
            logger.LogInformation(XmlUtility<trackResponse>.Serialize(response));
#endif
            return response;
        }
#endif
    }
}
