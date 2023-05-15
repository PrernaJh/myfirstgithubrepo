using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models.FileProcessing;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
	[System.Runtime.InteropServices.Guid("282892C9-0B47-4DA2-B37C-D259A8349EDC")]
	public class AsnFileProcessor : IAsnFileProcessor
	{
		private readonly ILogger<AsnFileProcessor> logger;
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IBinProcessor binProcessor;
		private readonly IPackageDuplicateProcessor packageDuplicateProcessor;
		private readonly IPackageRepository packageRepository;
		private readonly ISequenceProcessor sequenceProcessor;
		private readonly ISiteProcessor siteProcessor;
		private readonly IZipMapProcessor zipMapProcessor;
		private readonly IZipOverrideProcessor zipOverrideProcessor;
		private readonly IZoneProcessor zoneProcessor;

		public AsnFileProcessor(ILogger<AsnFileProcessor> logger,
			IActiveGroupProcessor activeGroupProcessor,
			IBinProcessor binProcessor,
			IPackageDuplicateProcessor packageDuplicateProcessor,
			IPackageRepository packageRepository,
			ISequenceProcessor sequenceProcessor,
			ISiteProcessor siteProcessor,
			IZipMapProcessor zipMapProcessor,
			IZipOverrideProcessor zipOverrideProcessor,
			IZoneProcessor zoneProcessor)
		{
			this.logger = logger;
			this.activeGroupProcessor = activeGroupProcessor;
			this.binProcessor = binProcessor;
			this.packageDuplicateProcessor = packageDuplicateProcessor;
			this.packageRepository = packageRepository;
			this.sequenceProcessor = sequenceProcessor;
			this.siteProcessor = siteProcessor;
			this.zipMapProcessor = zipMapProcessor;
			this.zipOverrideProcessor = zipOverrideProcessor;
			this.zoneProcessor = zoneProcessor;
		}

		public async Task<AsnFileImportResponse> ImportPackages(List<Package> packages, SubClient subClient, bool isDuplicateBlockEnabled, string webJobId)
		{
			var totalWatch = Stopwatch.StartNew();
			var response = new AsnFileImportResponse();

			if (packages.Any())
			{
				try
				{
					var getTotalWatch = Stopwatch.StartNew();
					var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);

					if (StringHelper.Exists(site.Id))
					{
						var activeGroupsWatch = Stopwatch.StartNew();
						var duplicatePackagesToUpdate = new List<Package>();
						var serviceRuleGroupId = await activeGroupProcessor.GetServiceRuleActiveGroupIdAsync(subClient.Name, site.TimeZone);
						var zipOverrideGroupIds = await activeGroupProcessor.GetZipOverrideActiveGroupIds(subClient.Name);						
						var binGroupId = await activeGroupProcessor.GetBinActiveGroupIdAsync(site.SiteName, site.TimeZone);
						var binMapGroupId = await activeGroupProcessor.GetBinMapActiveGroupIdAsync(subClient.Name, site.TimeZone);
						var zoneMapGroupId = await activeGroupProcessor.GetZoneMapActiveGroupIdAsync();
						var upsGeoDescGroupId = await activeGroupProcessor.GetUpsGeoDescActiveGroupId(site.SiteName, site.TimeZone);
						activeGroupsWatch.Stop();

						var getDomainWatch = Stopwatch.StartNew();
						var duplicateTimeSpan = new TimeSpan();

						// individual package data assignment
						foreach (var package in packages)
						{
							AssignSiteData(package, site);

							package.AsnImportWebJobId = webJobId;
							package.ZoneMapGroupId = zoneMapGroupId;
							package.ClientName = subClient.ClientName;
							package.SubClientName = subClient.Name;
							package.SubClientKey = subClient.Key;
							package.UspsPermitNumber = subClient.UspsPermitNo;
							package.ServiceRuleGroupId = serviceRuleGroupId;
							package.MailerId = StringHelper.Exists(subClient.UspsImpbMid) ? subClient.UspsImpbMid : "999999999";
							package.IsPoBox = IsPoBox(package);
							package.UpsGeoDescriptorGroupId = upsGeoDescGroupId;

							if (AddressUtility.IsNotInLower48States(package.State))
							{
								package.IsOutside48States = true;
							}

							if (isDuplicateBlockEnabled)
							{
								var duplicateWatch = Stopwatch.StartNew();
								await ProcessDuplicatesOrBlockPackage(duplicatePackagesToUpdate, package);
								duplicateWatch.Stop();
								duplicateTimeSpan = duplicateTimeSpan.Add(duplicateWatch.Elapsed);
							}
							package.PackageEvents.Add(new Event
							{
								EventId = package.PackageEvents.Count + 1,
								EventType = EventConstants.FileImport,
								EventStatus = package.PackageStatus,
								Description = $"ASN File Import",
								Username = "System",
								MachineId = "System",
								EventDate = DateTime.Now,
								LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
							});
						}

						// group data assignment
						var upsGeoDescWatch = Stopwatch.StartNew();
						await AssignUpsGeoDescToListOfPackages(packages, upsGeoDescGroupId);
						upsGeoDescWatch.Stop();

						var assignZipsWatch = Stopwatch.StartNew();
						await zipOverrideProcessor.AssignZipOverridesForListOfPackages(packages, zipOverrideGroupIds);
						assignZipsWatch.Stop();

						var binWatch = Stopwatch.StartNew();
						await binProcessor.AssignBinsForListOfPackagesAsync(packages, binGroupId, binMapGroupId);
						binWatch.Stop();

						var zonesWatch = Stopwatch.StartNew();
						await zoneProcessor.AssignZonesToListOfPackages(packages, site, zoneMapGroupId);
						zonesWatch.Stop();

						var sequenceWatch = Stopwatch.StartNew();
						await sequenceProcessor.AssignSequencesToListOfPackages(packages);
						sequenceWatch.Stop();
						getDomainWatch.Stop();

						response.ActiveGroupsTotalTime = TimeSpan.FromMilliseconds(activeGroupsWatch.ElapsedMilliseconds);
						response.AssignUpsGeoDescTotalTime = TimeSpan.FromMilliseconds(upsGeoDescWatch.ElapsedMilliseconds);
						response.AssignZipOverridesTotalTime = TimeSpan.FromMilliseconds(assignZipsWatch.ElapsedMilliseconds);
						response.TotalTime = TimeSpan.FromMilliseconds(getTotalWatch.ElapsedMilliseconds);
						response.DomainDataTotalTime = TimeSpan.FromMilliseconds(getDomainWatch.ElapsedMilliseconds);
						response.DuplicateCheckTotalTime = duplicateTimeSpan;
						response.ZonesTotalTime += TimeSpan.FromMilliseconds(zonesWatch.ElapsedMilliseconds);
						response.ZonesAverage = GetAverageFromTimeSpan(response.ZonesTotalTime, packages.Count);
						response.BinsTotalTime += TimeSpan.FromMilliseconds(binWatch.ElapsedMilliseconds);
						response.BinsAverage = GetAverageFromTimeSpan(response.BinsTotalTime, packages.Count);
						response.SequencesTotalTime += TimeSpan.FromMilliseconds(sequenceWatch.ElapsedMilliseconds);
						response.SequencesAverage = GetAverageFromTimeSpan(response.SequencesTotalTime, packages.Count);
						response.ZipOverrideAssignmentAverage = GetAverageFromTimeSpan(response.AssignZipOverridesTotalTime, packages.Count);
						response.DomainDataAverage = GetAverageFromTimeSpan(response.DomainDataTotalTime, packages.Count);
						response.DuplicateCheckAverage = GetAverageFromTimeSpan(response.DuplicateCheckTotalTime, packages.Count);

						var queryWatch = Stopwatch.StartNew();
						var bulkImportResponse = await packageRepository.AddItemsAsync(packages);
						var bulkUpdateResponse = new BatchDbResponse<Package>() { IsSuccessful = true };

						if (duplicatePackagesToUpdate.Any())
						{
							response.NumberOfDuplicates = duplicatePackagesToUpdate.Count();
							response.AverageDuplicatesPerPackage = (response.NumberOfDuplicates / packages.Count()).ToString();

							bulkUpdateResponse = await packageRepository.UpdateItemsAsync(duplicatePackagesToUpdate);
						}

						response.DbInsertTime = TimeSpan.FromMilliseconds(queryWatch.ElapsedMilliseconds);

						if (! bulkImportResponse.IsSuccessful || ! bulkUpdateResponse.IsSuccessful)
						{
							if (!bulkImportResponse.IsSuccessful)
								logger.Log(LogLevel.Error, $"Error importing {bulkImportResponse.FailedCount} new packages from ASN file. WebJobId: {webJobId}: {bulkImportResponse.Message}");
							if(! bulkUpdateResponse.IsSuccessful)
								logger.Log(LogLevel.Error, $"Error updating {bulkUpdateResponse.FailedCount} duplicate packages in ASN File. WebJobId: {webJobId}: {bulkUpdateResponse.Message}");
						}
						else
						{
							response.IsSuccessful = true;
						}
						response.NumberOfDocumentsImported += bulkImportResponse.Count;
						response.RequestUnitsConsumed += bulkImportResponse.RequestCharge;
					}
				}

				catch (Exception ex)
				{
					logger.Log(LogLevel.Error, $"Error processing ASN File. WebJobId: {webJobId} Exception: {ex}");
				}
			}

			response.TotalTime = TimeSpan.FromMilliseconds(totalWatch.ElapsedMilliseconds);
			return response;
		}

		public async Task<(List<Package> Packages, int NumberOfRecords)> ReadCmopFileStreamAsync(Stream stream)
		{
			try
			{
				var packages = new List<Package>();
				using (var reader = new StreamReader(stream))
				{
					while (!reader.EndOfStream)
					{
						var line = await reader.ReadLineAsync();
						var parts = line.Split('|');
						Package package = null;
						if (parts[0] == "V1")
						{
							package = ParseCmopV1Record(parts);
						}
                        else if (parts[0] == "V2")
						{
							package = ParseCmopV2Record(parts);
                        }
						if (package != null)
						{
							packages.Add(package);
						}
						else if (StringHelper.Exists(line.Trim()))
						{
							logger.LogError($"Exception: ASN Bad input data: {line}");
						}
					}
				}
				return (packages, packages.Count);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to read ASN file. Exception: { ex }");
				return (new List<Package>(), 0);
			}
		}

		public async Task<(List<Package> Packages, int NumberOfRecords)> ReadDalcFileStreamAsync(Stream stream)
		{
			try
			{
				var packages = new List<Package>();

				using (var reader = new StreamReader(stream))
				{
					while (!reader.EndOfStream)
					{
						var line = await reader.ReadLineAsync();
						var parts = line.Split('|');

						if (ValidateDalcAsnRecord(parts))
						{
							var packageId = parts[0].Trim();
							var fullZip = parts[6].Trim();

							packages.Add(new Package
							{
								Id = Guid.NewGuid().ToString(),
								PackageStatus = EventConstants.Imported,
								CreateDate = DateTime.UtcNow,
								PartitionKey = PartitionKeyUtility.GeneratePackagePartitionKeyString(packageId),
								PackageId = packageId,
								ContainerId = string.Empty,
								MailCode = "0",
								AddressLine1 = parts[1].Trim(),
								AddressLine2 = parts[2].Trim(),
								AddressLine3 = parts[3].Trim(),
								City = parts[4].Trim(),
								State = parts[5].Trim(),
								Zip = AddressUtility.TrimZipToFirstFive(fullZip),
								FullZip = fullZip,
								Length = 1,
								Depth = 1,
								Width = 1
							});
						}
						else if (StringHelper.Exists(line.Trim()))
						{
							logger.LogError($"Exception: ASN Bad input data: {line}");
						}
					}
				}
				return (packages, packages.Count);
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to read DALC file. Exception: { ex }");
				return (new List<Package>(), 0);
			}
		}

		private Package ParseCmopV1Record(string[] parts)
		{
			if (ValidateCmopRecord(parts))
			{
				var packageId = parts[1].Trim();
				var fullZip = parts[8].Trim();

				return new Package
				{
					Id = Guid.NewGuid().ToString(),
					PackageStatus = EventConstants.Imported,
					MailCode = PackageIdUtility.GenerateCmopMailCode(packageId),
					CreateDate = DateTime.UtcNow,
					PartitionKey = PartitionKeyUtility.GeneratePackagePartitionKeyString(packageId),
					PackageId = packageId,
					ContainerId = string.Empty,
					RecipientName = parts[2].Trim(),
					AddressLine1 = parts[3].Trim(),
					AddressLine2 = parts[4].Trim(),
					AddressLine3 = parts[5].Trim(),
					City = parts[6].Trim(),
					State = parts[7].Trim(),
					Zip = AddressUtility.TrimZipToFirstFive(fullZip),
					FullZip = fullZip,
					Phone = parts[9].Trim(),
					ReturnName = parts[10].Trim(),
					ReturnAddressLine1 = parts[11].Trim(),
					ReturnAddressLine2 = parts[12].Trim(),
					ReturnCity = parts[13].Trim(),
					ReturnState = parts[14].Trim(),
					ReturnZip = AddressUtility.TrimZipToFirstFive(parts[15]),
					ReturnPhone = parts[16].Trim(),
					Length = 1,
					Depth = 1,
					Width = 1
				};
			}
			return null;
		}

		private Package ParseCmopV2Record(string[] parts)
		{
			if (ValidateCmopRecord(parts))
			{
				var packageId = parts[1].Trim();
				var fullZip = parts[8].Trim();

				return new Package
				{
					Id = Guid.NewGuid().ToString(),
					PackageStatus = EventConstants.Imported,
					MailCode = PackageIdUtility.GenerateCmopMailCode(packageId),
					CreateDate = DateTime.UtcNow,
					PartitionKey = PartitionKeyUtility.GeneratePackagePartitionKeyString(packageId),
					PackageId = packageId,
					ContainerId = string.Empty,
					RecipientName = parts[2].Trim(),
					AddressLine1 = parts[3].Trim(),
					AddressLine2 = parts[4].Trim(),
					AddressLine3 = parts[5].Trim(),
					City = parts[6].Trim(),
					State = parts[7].Trim(),
					Zip = AddressUtility.TrimZipToFirstFive(fullZip),
					FullZip = fullZip,
					Phone = parts[9].Trim(),
					ReturnName = parts[10].Trim(),
					ReturnAddressLine1 = parts[11].Trim(),
					ReturnAddressLine2 = parts[12].Trim(),
					ReturnCity = parts[13].Trim(),
					ReturnState = parts[14].Trim(),
					ReturnZip = AddressUtility.TrimZipToFirstFive(parts[15]),
					ReturnPhone = parts[16].Trim(),
					Length = 1,
					Depth = 1,
					Width = 1
				};
			}
			return null;
		}

		private async Task ProcessDuplicatesOrBlockPackage(List<Package> duplicatePackagesToUpdate, Package package)
		{
			var duplicateImportedPackages = await packageDuplicateProcessor.GetDuplicatePackages(package); // siteName, packageid, and id are required
			duplicatePackagesToUpdate.AddRange(duplicateImportedPackages);
		}

		private async Task AssignUpsGeoDescToListOfPackages(List<Package> packages, string upsGeoDescGroupId)
		{
			var groupedPackages = packages.GroupBy(x => x.Zip);

			foreach (var groupOfPackages in groupedPackages)
			{
				var zipMap = await zipMapProcessor.GetZipMapByGroupAsync(upsGeoDescGroupId, groupOfPackages.Key);

				foreach (var package in groupOfPackages)
				{
					package.UpsGeoDescriptor = zipMap.Value;
				}
			}
		}

		private static bool ValidateCmopRecord(string[] parts)
		{
			var result = false;
			if (parts.Length > 16)
			{
				return true;
			}
			return result;
		}

		private static bool ValidateDalcAsnRecord(string[] parts)
		{
			return parts.Length > 6 && StringHelper.Exists(parts[0]);
		}

		private void AssignSiteData(Package package, Site site)
		{
			package.SiteId = site.Id;
			package.SiteName = site.SiteName;
			package.SiteZip = site.Zip;
			package.SiteAddressLineOne = site.AddressLineOne;
			package.SiteCity = site.City;
			package.SiteState = site.State;
			package.TimeZone = site.TimeZone;
		}

		private bool IsPoBox(Package package)
		{
			var pattern = AddressConstants.PoBoxRegex;

			if (Regex.IsMatch(package.AddressLine1, pattern, RegexOptions.IgnoreCase))
			{
				return true;
			}

			return false;
		}

		private string GetAverageFromTimeSpan(TimeSpan span, int count)
		{
			var response = span.TotalMilliseconds / count;
			return response.ToString();
		}
	}
}