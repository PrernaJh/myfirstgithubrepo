using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models.RecallRelease;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
	public class RecallReleaseProcessor : IRecallReleaseProcessor
	{
		private readonly ILogger<RecallReleaseProcessor> logger;
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IBinProcessor binProcessor;
		private readonly IEodPostProcessor eodPostProcessor;
		private readonly IPackagePostProcessor packagePostProcessor;
		private readonly IPackageRepository packageRepository;
		private readonly ISiteProcessor siteProcessor;
		private readonly ISubClientProcessor subClientProcessor;

		public RecallReleaseProcessor(ILogger<RecallReleaseProcessor> logger,
			IActiveGroupProcessor activeGroupProcessor,
			IBinProcessor binProcessor,
			IEodPostProcessor eodPostProcessor,
			IPackagePostProcessor packagePostProcessor,
			IPackageRepository packageRepository,
			ISiteProcessor siteProcessor,
			ISubClientProcessor subClientProcessor)
		{

			this.logger = logger;
			this.activeGroupProcessor = activeGroupProcessor;
			this.binProcessor = binProcessor;
			this.eodPostProcessor = eodPostProcessor;
			this.packagePostProcessor = packagePostProcessor;
			this.packageRepository = packageRepository;
			this.siteProcessor = siteProcessor;
			this.subClientProcessor = subClientProcessor;
		}

		public async Task<RecallReleasePackageResponse> ImportListOfRecallPackagesForSubClient(Stream stream, string subClientName, string username)
		{
			try
			{
				var response = new RecallReleasePackageResponse();
				var fileReadWatch = Stopwatch.StartNew();
				var packageIds = new List<string>();
				packageIds.AddRange(await ReadRecallFileStreamAsync(stream));

				logger.Log(LogLevel.Information, $"Recall file rows read: { packageIds.Count }");

				if (packageIds.Any())
				{
					await RecallListOfPackages(subClientName, username, packageIds, response);
				}

				return response;
			}

			catch (Exception ex)
			{
				logger.LogError($"Error processing Recall File. Exception: {ex}");
				return new RecallReleasePackageResponse();
			}
		}

		public async Task<RecallReleasePackageResponse> ImportListOfReleasePackagesForSubClient(Stream stream, string subClientName, string username)
		{
			try
			{
				var response = new RecallReleasePackageResponse();
				var fileReadWatch = Stopwatch.StartNew();
				var packageIds = new List<string>();
				packageIds.AddRange(await ReadReleaseFileStreamAsync(stream));

				logger.Log(LogLevel.Information, $"Release file rows read: { packageIds.Count }");

				if (packageIds.Any())
				{
					await ReleaseListOfPackages(subClientName, username, packageIds, response);
				}

				return response;
			}

			catch (Exception ex)
			{
				logger.LogError($"Error processing Recall File. Exception: {ex}");
				return new RecallReleasePackageResponse();
			}
		}

		public async Task<RecallReleasePackageResponse> ProcessRecallPackageForSubClient(string packageId, string subClientName, string username)
		{
			var response = new RecallReleasePackageResponse();
			await RecallPackage(subClientName, username, packageId, response);
			return response;
		}

		public async Task<RecallReleasePackageResponse> ProcessReleasePackageForSubClient(string packageId, string subClientName, string username)
		{
			var response = new RecallReleasePackageResponse();
			await ReleasePackage(subClientName, username, packageId, response);
			return response;
		}

		public async Task<RecallReleasePackageResponse> ProcessDeleteRecallPackageForSubClient(string packageId, string subClientName, string username)
		{
			var response = new RecallReleasePackageResponse();
			await DeleteRecallPackage(subClientName, username, packageId, response);
			return response;
		}

		public async Task<IEnumerable<Package>> GetRecalledPackagesAsync(string subClientName)
		{
			return await packageRepository.GetRecalledPackages(subClientName);
		}

		public async Task<IEnumerable<Package>> GetReleasedPackagesAsync(string subClientName)
		{
			return await packageRepository.GetReleasedPackages(subClientName);
		}

		public async Task<IEnumerable<Package>> FindPackagesToRecallByPartial(string subClientName, string partialPackageId)
		{
			var packages = await packageRepository.GetPackagesToRecallByPartial(subClientName, partialPackageId);
			var result = new List<Package>();
			// Remove duplicates
			foreach (var group in packages.GroupBy(p => p.PackageId))
				result.Add(group.FirstOrDefault());
			return result;
		}

		private async Task<Package> GetPackageToRecallBySubClient(string siteName, string subClientName, string packageId)
		{
			var package = await packagePostProcessor.GetPackageByPackageId(packageId, siteName);
			return package.SubClientName == subClientName ? package : new Package();
		}

		private async Task<Package> GetPackageToReleaseBySubClient(string siteName, string subClientName, string packageId)
		{
			var package = await packagePostProcessor.GetPackageByPackageId(packageId, siteName);
			return package.SubClientName == subClientName && package.PackageStatus == EventConstants.Recalled ? package : new Package();
		}		

		private async Task RecallPackage(string subClientName, string username, string packageId, RecallReleasePackageResponse response)
		{
			try
			{
				var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
				var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
				var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var binGroupId = await activeGroupProcessor.GetBinActiveGroupIdAsync(subClient.SiteName, site.TimeZone);
				var binMapGroupId = await activeGroupProcessor.GetBinMapActiveGroupIdAsync(subClient.Name, site.TimeZone);
				var package = await GetPackageToRecallBySubClient(site.SiteName, subClientName, packageId);
				bool isNew = false;
				if (StringHelper.DoesNotExist(package.PackageId))
				{
					// Make sure there isn't already a package with this id at this site
					var otherPackage = await packagePostProcessor.GetPackageByPackageId(packageId, site.SiteName);
					if (StringHelper.DoesNotExist(otherPackage.PackageId))
                    {
						package = await CreatePackageForRecall(site, subClient, username, packageId);
						isNew = true;
                    }
				}
				if ((package.PackageStatus == EventConstants.Imported ||
					package.PackageStatus == EventConstants.Processed ||
					package.PackageStatus == EventConstants.Released ||
					package.PackageStatus == EventConstants.Deleted ||
					isNew) &&
					!package.IsCreated)
				{
					package.IsLocked = package.PackageStatus == EventConstants.Processed &&
						await eodPostProcessor.ShouldLockPackage(package);
					if (!package.IsLocked)
					{
						if (!isNew)
						package.RecallStatus = GetRecallStatus(package);
						package.PackageStatus = EventConstants.Recalled;
						package.RecallDate = siteLocalTime;
						package.ReleaseDate = DateTime.MinValue;
						package.EodUpdateCounter += 1;
						var packageEventId = package.PackageEvents.Count + 1;
						package.PackageEvents.Add(new Event
						{
							EventId = packageEventId,
							EventType = EventConstants.ManualRecall,
							EventStatus = package.PackageStatus,
							Description = $"{package.PackageStatus} by Web User",
							Username = username,
							MachineId = "Web",
							EventDate = DateTime.Now,
							LocalEventDate = siteLocalTime
						});
						response.PackageIds.Add(package.PackageId);
						response.Packages.Add(package);
						response.IsSuccessful = true;
					}
					else
					{
						package.PackageEvents.Add(new Event
						{
							EventId = package.PackageEvents.Count + 1,
							EventType = EventConstants.ManualRecall,
							EventStatus = package.PackageStatus,
							Description = "Package locked by end of day file process",
							Username = username,
							MachineId = "Web",
							EventDate = DateTime.Now,
							LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
						});
						response.LockedPackageIds.Add(package.PackageId);
					}
					if (StringHelper.Exists(package.Zip)
						&& (package.BinGroupId != binGroupId || package.BinMapGroupId != binMapGroupId))
					{
						await binProcessor.AssignBinsForListOfPackagesAsync(new List<Package>() { package }, binGroupId, binMapGroupId, true);
					}
					await packageRepository.UpdateItemAsync(package);					
				}
				else
				{
					logger.LogInformation($"Did not find PackageId to Recall: {packageId} for SubClient: {subClientName} UserName: {username}");
					response.FailedPackageIds.Add(packageId);
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to Recall packageId {packageId} for SubClient: {subClientName} UserName: {username} Exception: {ex}");
				response.FailedPackageIds.Add(packageId);
			}
		}

		private async Task ReleasePackage(string subClientName, string username, string packageId, RecallReleasePackageResponse response)
		{
			try
			{
				var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
				var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
				var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var binGroupId = await activeGroupProcessor.GetBinActiveGroupIdAsync(subClient.SiteName, site.TimeZone);
				var binMapGroupId = await activeGroupProcessor.GetBinMapActiveGroupIdAsync(subClient.Name, site.TimeZone);
				var package = await GetPackageToReleaseBySubClient(site.SiteName, subClientName, packageId);
				if (StringHelper.Exists(package.PackageId))
				{
					package.RecallStatus = EventConstants.Released;
					package.PackageStatus = EventConstants.Released;
					package.ReleaseDate = siteLocalTime;
					package.EodUpdateCounter += 1;
					var packageEventId = package.PackageEvents.Count + 1;
					package.PackageEvents.Add(new Event
					{
						EventId = packageEventId,
						EventType = EventConstants.ManualRelease,
						EventStatus = package.PackageStatus,
						Description = $"{package.PackageStatus} by Web User",
						Username = username,
						MachineId = "Web",
						EventDate = DateTime.Now,
						LocalEventDate = siteLocalTime
					});
					response.PackageIds.Add(package.PackageId);
					response.Packages.Add(package);
					response.IsSuccessful = true;
					if (StringHelper.Exists(package.Zip)
						&& (package.BinGroupId != binGroupId || package.BinMapGroupId != binMapGroupId))
					{
						await binProcessor.AssignBinsForListOfPackagesAsync(new List<Package>() { package }, binGroupId, binMapGroupId, true);
					}
					await packageRepository.UpdateItemAsync(package);
				}
				else
				{
					logger.LogInformation($"Did not find PackageId to Release: {packageId} for SubClient: {subClientName} UserName: {username}");
					response.FailedPackageIds.Add(packageId);
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to Release packageId {packageId} for SubClient: {subClientName} UserName: {username} Exception: {ex}");
				response.FailedPackageIds.Add(packageId);
			}
		}

		private async Task DeleteRecallPackage(string subClientName, string username, string packageId, RecallReleasePackageResponse response)
		{
			try
			{
				var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
				var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
				var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var package = await GetPackageToRecallBySubClient(site.SiteName, subClientName, packageId);
				
				if (package.RecallStatus == EventConstants.RecallCreated && package.PackageStatus == EventConstants.Recalled)
				{
					package.IsLocked = package.PackageStatus == EventConstants.Processed &&
						await eodPostProcessor.ShouldLockPackage(package);
					if (!package.IsLocked)
					{
						package.PackageStatus = EventConstants.Deleted;
						package.ReleaseDate = DateTime.MinValue;
						package.EodUpdateCounter += 1;
						var packageEventId = package.PackageEvents.Count + 1;
						package.PackageEvents.Add(new Event
						{
							EventId = packageEventId,
							EventType = EventConstants.ManualDelete,
							EventStatus = EventConstants.Deleted,
							Description = $"{EventConstants.Deleted} by Web User",
							Username = username,
							MachineId = "Web",
							EventDate = DateTime.Now,
							LocalEventDate = siteLocalTime
						});
						response.PackageIds.Add(package.PackageId);
						response.Packages.Add(package);
						response.IsSuccessful = true;
					}
					else
					{
						package.PackageEvents.Add(new Event
						{
							EventId = package.PackageEvents.Count + 1,
							EventType = EventConstants.ManualRecall,
							EventStatus = package.PackageStatus,
							Description = "Package locked by end of day file process",
							Username = username,
							MachineId = "Web",
							EventDate = DateTime.Now,
							LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
						});
						response.LockedPackageIds.Add(package.PackageId);
					}
					await packageRepository.UpdateItemAsync(package);
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to Delete packageId {packageId} for SubClient: {subClientName} UserName: {username} Exception: {ex}");
				response.FailedPackageIds.Add(packageId);
			}
		}

		private async Task<Package> CreatePackageForRecall(Site site, SubClient subClient, string username, string packageId)
        {
			var package = await packageRepository.AddItemAsync(new Package()
			{
				PackageId = packageId,
				PackageStatus = EventConstants.Recalled,
				RecallStatus = EventConstants.RecallCreated,
				ClientName = subClient.ClientName,
				SiteId = site.Id,
				SiteName = site.SiteName,
				SiteZip = site.Zip,
				SiteAddressLineOne = site.AddressLineOne,
				SiteState = site.State,
				TimeZone = site.TimeZone,
				SubClientName = subClient.Name,
				SubClientKey = subClient.Key,
				CreateDate = DateTime.Now,
			}, packageId);
			logger.LogInformation($"Recalled package Added for PackageId: {packageId} for SubClient: {subClient.Name} UserName: {username}");
			return package;
		}

		private static string GetRecallStatus(Package package)
        {
			var recallStatus = string.Empty;
			if (package.PackageStatus == EventConstants.Released || package.PackageStatus == EventConstants.Deleted)
			{
				if (package.PackageEvents
						.FirstOrDefault(e => e.EventType == EventConstants.ManualScan && e.EventStatus == EventConstants.Recalled) != null)
					recallStatus = EventConstants.RecallScanned;
				else if (package.PackageEvents
						.FirstOrDefault(e => e.EventType == EventConstants.AutoScan && e.EventStatus == EventConstants.Recalled) != null)
					recallStatus = EventConstants.RecallScanned;
				else if (package.PackageEvents.FirstOrDefault(e => e.EventStatus == EventConstants.Processed) != null)
					recallStatus = EventConstants.Processed;
				else if (package.PackageEvents.FirstOrDefault(e => e.EventType == EventConstants.FileImport) != null)
					recallStatus = EventConstants.Imported;
				else
					recallStatus = EventConstants.RecallCreated;
			}
			else
			{
				recallStatus = package.PackageStatus;
			}
			return recallStatus;
		}

		private async Task RecallListOfPackages(string subClientName, string username, List<string> packageIds, RecallReleasePackageResponse response)
		{
			try
			{
				var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
				var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
				var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var binGroupId = await activeGroupProcessor.GetBinActiveGroupIdAsync(subClient.SiteName, site.TimeZone);
				var binMapGroupId = await activeGroupProcessor.GetBinMapActiveGroupIdAsync(subClient.Name, site.TimeZone);
				var packagesForBinUpdate = new List<Package>();
				foreach (var packageId in packageIds)
				{
					var package = await GetPackageToRecallBySubClient(site.SiteName, subClientName, packageId);
					bool isNew = false;
					if (StringHelper.DoesNotExist(package.PackageId))
					{
						// Make sure there isn't already a package with this id at this site
						var otherPackage = await packagePostProcessor.GetPackageByPackageId(packageId, site.SiteName);
						if (StringHelper.DoesNotExist(otherPackage.PackageId))
						{
							package = await CreatePackageForRecall(site, subClient, username, packageId);
							isNew = true;
						}
					}
					if ((package.PackageStatus == EventConstants.Imported || 
						package.PackageStatus == EventConstants.Processed || 
						package.PackageStatus == EventConstants.Released ||
						package.PackageStatus == EventConstants.Deleted ||
						isNew) &&
						!package.IsCreated)
					{
						package.IsLocked = package.PackageStatus == EventConstants.Processed &&
							await eodPostProcessor.ShouldLockPackage(package);
						if (!package.IsLocked)
						{
							if (!isNew)
								package.RecallStatus = GetRecallStatus(package);
							package.PackageStatus = EventConstants.Recalled;
							package.RecallDate = siteLocalTime;
							package.ReleaseDate = DateTime.MinValue;
							package.EodUpdateCounter += 1;
							var packageEventId = package.PackageEvents.Count + 1;
							package.PackageEvents.Add(new Event
							{
								EventId = packageEventId,
								EventType = EventConstants.ManualRecall,
								EventStatus = package.PackageStatus,
								Description = $"{package.PackageStatus} by Web User",
								Username = username,
								MachineId = "Web",
								EventDate = DateTime.Now,
								LocalEventDate = siteLocalTime
							});
							response.PackageIds.Add(package.PackageId);
							if (StringHelper.Exists(package.Zip)
								&& (package.BinGroupId != binGroupId || package.BinMapGroupId != binMapGroupId))
							{
								packagesForBinUpdate.Add(package);
							}
						}
						else
						{
							package.PackageEvents.Add(new Event
							{
								EventId = package.PackageEvents.Count + 1,
								EventType = EventConstants.ManualRecall,
								EventStatus = package.PackageStatus,
								Description = "Package locked by end of day file process",
								Username = username,
								MachineId = "Web",
								EventDate = DateTime.Now,
								LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
							});
							response.LockedPackageIds.Add(package.PackageId);
						}
						response.Packages.Add(package);
					}
					else
					{
						logger.LogInformation($"Did not find PackageId to Recall: {packageId} for SubClient: {subClientName} UserName: {username}");
						response.FailedPackageIds.Add(packageId);
					}
				}

				await binProcessor.AssignBinsForListOfPackagesAsync(packagesForBinUpdate, binGroupId, binMapGroupId, true);
				var bulkResponse = await packageRepository.UpdatePackagesSetBinData(packagesForBinUpdate);
				if (bulkResponse.IsSuccessful)
				{
					bulkResponse = await packageRepository.UpdatePackagesForRecallRelease(response.Packages);
					if (bulkResponse.IsSuccessful)
					{
						response.IsSuccessful = true;
					}
					else
					{
						logger.LogError($"Failed to bulk update recall packages for SubClient: {subClientName} UserName: {username}");
					}				
				}
				else
				{
					logger.LogError($"Failed to bulk update recall packages for SubClient: {subClientName} UserName: {username}");
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to recall packages for SubClient: {subClientName} UserName: {username} Exception: {ex}");
				response.FailedPackageIds.AddRange(response.PackageIds);
				response.PackageIds.Clear();
				response.Packages.Clear();
			}
		}

		private async Task ReleaseListOfPackages(string subClientName, string username, List<string> packageIds, RecallReleasePackageResponse response)
		{
			try
			{
				var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
				var site = await siteProcessor.GetSiteBySiteNameAsync(subClient.SiteName);
				var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var binGroupId = await activeGroupProcessor.GetBinActiveGroupIdAsync(subClient.SiteName, site.TimeZone);
				var binMapGroupId = await activeGroupProcessor.GetBinMapActiveGroupIdAsync(subClient.Name, site.TimeZone);
				var packagesForBinUpdate = new List<Package>();
				foreach (var packageId in packageIds)
				{
					var package = await GetPackageToReleaseBySubClient(site.SiteName, subClientName, packageId);
					if (StringHelper.Exists(package.PackageId))
					{
						package.RecallStatus = EventConstants.Released;					
						package.PackageStatus = EventConstants.Released;
						package.RecallDate = DateTime.MinValue;
						package.ReleaseDate = siteLocalTime;
						package.EodUpdateCounter += 1;
						var packageEventId = package.PackageEvents.Count + 1;
						package.PackageEvents.Add(new Event
						{
							EventId = packageEventId,
							EventType = EventConstants.ManualRelease,
							EventStatus = package.PackageStatus,
							Description = $"{package.PackageStatus} by Web User",
							Username = username,
							MachineId = "Web",
							EventDate = DateTime.Now,
							LocalEventDate = siteLocalTime
						});
						response.PackageIds.Add(package.PackageId); 
						response.Packages.Add(package);
						if (StringHelper.Exists(package.Zip)
							&& (package.BinGroupId != binGroupId || package.BinMapGroupId != binMapGroupId))
						{
							packagesForBinUpdate.Add(package);
						}
					}
					else
					{
						logger.LogInformation($"Did not find PackageId to Release: {packageId} for SubClient: {subClientName} UserName: {username}");
						response.FailedPackageIds.Add(packageId);
					}
				}

				await binProcessor.AssignBinsForListOfPackagesAsync(packagesForBinUpdate, binGroupId, binMapGroupId, true);
				var bulkResponse = await packageRepository.UpdatePackagesSetBinData(packagesForBinUpdate);
				if (bulkResponse.IsSuccessful)
				{
					bulkResponse = await packageRepository.UpdatePackagesForRecallRelease(response.Packages);
					if (bulkResponse.IsSuccessful)
					{
						response.IsSuccessful = true;
					}
					else
					{
						logger.LogError($"Failed to bulk update release packages for SubClient: {subClientName} UserName: {username}");
					}
				}
				else
				{
					logger.LogError($"Failed to bulk update release packages for SubClient: {subClientName} UserName: {username}");
				}			}
			catch (Exception ex)
			{
				logger.LogError($"Failed to release packages for SubClient: {subClientName} UserName: {username} Exception: {ex}");
				response.FailedPackageIds.AddRange(response.PackageIds);
				response.PackageIds.Clear();
				response.Packages.Clear();
			}
		}

		private async Task<List<string>> ReadRecallFileStreamAsync(Stream stream)
		{
			var packageIds = new List<string>();
			using (var reader = new StreamReader(stream))
			{
				while (!reader.EndOfStream)
				{
					var line = await reader.ReadLineAsync();
					foreach (var part in line.Trim('\n').Split(','))
					{
						if (StringHelper.Exists(part.Trim()))
							packageIds.Add(part.Trim());
					}
				}
			}
			return packageIds;
		}

		private async Task<List<string>> ReadReleaseFileStreamAsync(Stream stream)
		{
			var packageIds = new List<string>();
			using (var reader = new StreamReader(stream))
			{
				while (!reader.EndOfStream)
				{
					var line = await reader.ReadLineAsync();
					foreach (var part in line.Trim('\n').Split(','))
					{
						if (StringHelper.Exists(part.Trim()))
							packageIds.Add(part.Trim());
					}
				}
			}
			return packageIds;
		}
    }
}
