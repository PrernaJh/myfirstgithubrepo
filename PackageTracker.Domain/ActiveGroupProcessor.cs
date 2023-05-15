using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models.ActiveGroupDetails;
using PackageTracker.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Domain
{
    public class ActiveGroupProcessor : IActiveGroupProcessor
    {
        private readonly IActiveGroupRepository activeGroupRepository;
        private readonly ISiteProcessor siteProcessor;
        private readonly ISubClientProcessor subClientProcessor;

        public ActiveGroupProcessor(IActiveGroupRepository activeGroupRepository, ISiteProcessor siteProcessor, ISubClientProcessor subClientProcessor)
        {
            this.activeGroupRepository = activeGroupRepository;
            this.siteProcessor = siteProcessor;
            this.subClientProcessor = subClientProcessor;
        }

        public async Task<DateTime> GetLocalTimeBySite(string siteName, string timeZone = null)
        {
            var localDateTime = DateTime.Now;
            if (timeZone != null)
            {
                localDateTime = TimeZoneUtility.GetLocalTime(timeZone);
            }
            else
            {
                var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
                localDateTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
            }
            return localDateTime;
        }

        public async Task<DateTime> GetLocalTimeBySubClient(string subClientName, string timeZone = null)
        {
            var localDateTime = DateTime.Now;
            if (timeZone != null)
            {
                localDateTime = TimeZoneUtility.GetLocalTime(timeZone);
            }
            else
            {
                var subClient = await subClientProcessor.GetSubClientByNameAsync(subClientName);
                localDateTime = await GetLocalTimeBySite(subClient.SiteName);
            }
            return localDateTime;
        }

		public async Task<ActiveGroup> GetActiveGroupByIdAsync(string id, string name)
		{
			return await activeGroupRepository.GetItemAsync(id, name) ?? new ActiveGroup();
		}

		public async Task<ActiveGroup> AddActiveGroupAsync(ActiveGroup activeGroup)
		{
			return await activeGroupRepository.AddItemAsync(activeGroup, activeGroup.Name);
		}

        // get current group by site

        public async Task<string> GetBinActiveGroupIdAsync(string siteName, string timeZone = null)
        {
            var activeGroupType = ActiveGroupTypeConstants.Bins;
            var localDateTime = await GetLocalTimeBySite(siteName, timeZone);
            return await activeGroupRepository.GetCurrentActiveGroupIdAsync(activeGroupType, siteName, localDateTime);
        }

        public async Task<string> GetBinActiveGroupIdByDateAsync(string siteName, DateTime? dateTime, string timeZone)
        {
            var activeGroupType = ActiveGroupTypeConstants.Bins;
            var dateToSearch = dateTime ?? await GetLocalTimeBySite(siteName, timeZone);
            return await activeGroupRepository.GetCurrentActiveGroupIdAsync(activeGroupType, siteName, dateToSearch);
        }

        public async Task<string> GetUpsGeoDescActiveGroupId(string siteName, string timeZone = null)
        {
            var activeGroupType = ActiveGroupTypeConstants.UpsGeoDescriptors;
            var localDateTime = await GetLocalTimeBySite(siteName, timeZone);
            return await activeGroupRepository.GetCurrentActiveGroupIdAsync(activeGroupType, siteName, localDateTime);
        }

        public async Task<string> GetContainerRatesActiveGroupIdAsync(string siteName, string timeZone = null)
        {
            var activeGroupType = ActiveGroupTypeConstants.ContainerRates;
            var localDateTime = await GetLocalTimeBySite(siteName, timeZone);
            return await activeGroupRepository.GetCurrentActiveGroupIdAsync(activeGroupType, siteName, localDateTime);
        }

        public async Task<IEnumerable<ActiveGroup>> GetContainerRatesActiveGroupsAsync(string siteName)
        {
            var activeGroupType = ActiveGroupTypeConstants.ContainerRates;
            return await activeGroupRepository.GetActiveGroupsByTypeAsync(activeGroupType, siteName);
        }

        // get current by subClient

        public async Task<string> GetBinMapActiveGroupIdAsync(string subClientName, string timeZone = null)
        {
            var activeGroupType = ActiveGroupTypeConstants.BinMaps;
            var localDateTime = await GetLocalTimeBySubClient(subClientName, timeZone);
            return await activeGroupRepository.GetCurrentActiveGroupIdAsync(activeGroupType, subClientName, localDateTime);
        }

        public async Task<string> GetBinMapActiveGroupIdByDateAsync(string subClientName, DateTime? dateTime, string timeZone)
        {
            var activeGroupType = ActiveGroupTypeConstants.BinMaps;
            var dateToSearch = dateTime ?? await GetLocalTimeBySite(subClientName, timeZone);
            return await activeGroupRepository.GetCurrentActiveGroupIdAsync(activeGroupType, subClientName, dateToSearch);
        }

        public async Task<string> GetServiceRuleActiveGroupIdAsync(string subClientName, string timeZone = null)
        {
            var activeGroupType = ActiveGroupTypeConstants.ServiceRules;
            var localDateTime = await GetLocalTimeBySubClient(subClientName, timeZone);
            return await activeGroupRepository.GetCurrentActiveGroupIdAsync(activeGroupType, subClientName, localDateTime);
        }

        public async Task<string> GetServiceRuleActiveGroupIdByDateAsync(string subClientName, DateTime? dateTime, string timeZone)
        {
            var activeGroupType = ActiveGroupTypeConstants.ServiceRules;
            var dateToSearch = dateTime ?? await GetLocalTimeBySubClient(subClientName, timeZone);
            return await activeGroupRepository.GetCurrentActiveGroupIdAsync(activeGroupType, subClientName, dateToSearch);
        }

		public async Task<List<ActiveGroup>> GetServiceOverrideActiveGroupsAsync(string subClientName, string packageTimeZone)
		{
			var activeGroupType = ActiveGroupTypeConstants.ServiceOverrides;
			var localDateTime = TimeZoneUtility.GetLocalTime(packageTimeZone);

			var activeGroups = await activeGroupRepository.GetCurrentActiveGroupsWithEndDateAsync(activeGroupType, subClientName, localDateTime);
			return activeGroups.ToList();
		}

        public async Task<string> GetRatesActiveGroupIdAsync(string subClientName, string timeZone = null)
        {
            var activeGroupType = ActiveGroupTypeConstants.Rates;
            var localDateTime = await GetLocalTimeBySubClient(subClientName, timeZone);
            return await activeGroupRepository.GetCurrentActiveGroupIdAsync(activeGroupType, subClientName, localDateTime);
        }

        public async Task<string> GetZipCarrierOverrideActiveGroupIdAsync(string subClientName, string timeZone = null)
        {
            var activeGroupType = ActiveGroupTypeConstants.ZipCarrierOverride;
            var localDateTime = await GetLocalTimeBySubClient(subClientName, timeZone);
            return await activeGroupRepository.GetCurrentActiveGroupIdAsync(activeGroupType, subClientName, localDateTime);
        }

        // get current by client

        public async Task<string> GetFortyEightStatesActiveGroupIdAsync(string clientName)
        {
            var activeGroupType = ActiveGroupTypeConstants.FortyEightStates;
            return await activeGroupRepository.GetCurrentActiveGroupIdAsync(activeGroupType, clientName, DateTime.Now);
        }

        // global

        public async Task<string> GetZipMapActiveGroup(string activeGroupType)
        {
            return await activeGroupRepository.GetCurrentActiveGroupIdAsync(activeGroupType, SiteConstants.AllSites, DateTime.Now);
        }

        public async Task<string> GetZoneMapActiveGroupIdAsync()
        {
            var activeGroupType = ActiveGroupTypeConstants.ZoneMaps;
            return await activeGroupRepository.GetCurrentActiveGroupIdAsync(activeGroupType, SiteConstants.AllSites, DateTime.Now);
        }

        public async Task<List<string>> GetZipOverrideActiveGroupIds(string subClientName)
        {
            var currentZipGroupIds = new List<string>();
            var zipOverrideGlobalTypes = new List<string>
            {
                ActiveGroupTypeConstants.ZipsUpsSat,
                ActiveGroupTypeConstants.ZipsUpsSat48,
                ActiveGroupTypeConstants.ZipsUpsDas,
                ActiveGroupTypeConstants.ZipsFedExHawaii, 
                ActiveGroupTypeConstants.ZipsUspsRural
            };

            var zipOverrideSubClientTypes = new List<string>
            {
                ActiveGroupTypeConstants.ZipCarrierOverride
            };

            foreach (var zipType in zipOverrideGlobalTypes)
            {
                var zipOverrideGlobalGroupIds = await activeGroupRepository.GetCurrentActiveGroupIdAsync(zipType, SiteConstants.AllSites, DateTime.Now);

                if (StringHelper.Exists(zipOverrideGlobalGroupIds))
                {
                    currentZipGroupIds.Add(zipOverrideGlobalGroupIds);
                }
            }

            foreach (var zipType in zipOverrideSubClientTypes)
            {
                var zipOverrideSubClientGroupIds = await activeGroupRepository.GetCurrentActiveGroupIdAsync(zipType, subClientName, DateTime.Now);

                if (StringHelper.Exists(zipOverrideSubClientGroupIds))
                {
                    currentZipGroupIds.Add(zipOverrideSubClientGroupIds);
                }
            }

            return currentZipGroupIds;
        }

        // get all groups by type for historical views

		public async Task<IEnumerable<ActiveGroup>> GetShippingMethodOverrideActiveGroupsAsync(string subClientName)
		{
			var activeGroupType = ActiveGroupTypeConstants.ServiceOverrides;
			return await activeGroupRepository.GetActiveGroupsByTypeAsync(activeGroupType, subClientName);
		}

        public async Task<IEnumerable<ActiveGroup>> GetZipOverrideActiveGroupsAsync(string subClientName)
        {
            var activeGroupType = ActiveGroupTypeConstants.ZipCarrierOverride;
            return await activeGroupRepository.GetActiveGroupsByTypeAsync(activeGroupType, subClientName);
        }

        public async Task<IEnumerable<ActiveGroup>> GetAllRateActiveGroupsAsync(string subClientName)
        {
            var activeGroupType = ActiveGroupTypeConstants.Rates;
            return await activeGroupRepository.GetActiveGroupsByTypeAsync(activeGroupType, subClientName);
        }

        public async Task<IEnumerable<ActiveGroup>> GetAllActiveGroupsByType(string activeGroupType, bool isGlobal, string name = null)
        {
            var nameToQuery = string.Empty;

            if (isGlobal)
            {
                nameToQuery = "GLOBAL";
            }
            else
            {
                nameToQuery = name;
            }
            return await activeGroupRepository.GetActiveGroupsByTypeAsync(activeGroupType, nameToQuery);
        }

        public async Task<IEnumerable<ActiveGroup>> GetServiceRuleActiveGroupsAsync(string subClientName)
        {
            var activeGroupType = ActiveGroupTypeConstants.ServiceRules;
            return await activeGroupRepository.GetActiveGroupsByTypeAsync(activeGroupType, subClientName);
        }

        public async Task<IEnumerable<ActiveGroup>> GetFortyEightStatesActiveGroupsAsync(string subClientName)
        {
            var activeGroupType = ActiveGroupTypeConstants.FortyEightStates;
            return await activeGroupRepository.GetActiveGroupsByTypeAsync(activeGroupType, subClientName);
        }


        public async Task<IEnumerable<ActiveGroup>> GetBinSchemaActiveGroupsAsync(string siteName)
        {
            var activeGroupType = ActiveGroupTypeConstants.Bins;
            return await activeGroupRepository.GetActiveGroupsByTypeAsync(activeGroupType, siteName);
        }

        public async Task<IEnumerable<ActiveGroup>> GetBinMapsActiveGroupsAsync(string subClient)
        {
            var activeGroupType = ActiveGroupTypeConstants.BinMaps;
            return await activeGroupRepository.GetActiveGroupsByTypeAsync(activeGroupType, subClient);
        }

        // get active group upload details

        public async Task<GetServiceRuleDetailsResponse> GetServiceRuleDetails()
        {
            var response = new GetServiceRuleDetailsResponse();

            var subClients = await subClientProcessor.GetSubClientsAsync();

            foreach (var subClient in subClients)
            {
                var currentActiveGroupId = await GetServiceRuleActiveGroupIdAsync(subClient.Name);

                if (StringHelper.Exists(currentActiveGroupId))
                {
                    var activeGroup = await GetActiveGroupByIdAsync(currentActiveGroupId, subClient.Name);

                    if (StringHelper.Exists(activeGroup.Id))
                    {
                        response.ServiceRuleDetails.Add(new ServiceRuleDetails
                        {
                            SubClientName = subClient.Name,
                            ActiveGroupId = activeGroup.Id,
                            StartDate = activeGroup.StartDate.ToString(),
                            CreateDate = activeGroup.CreateDate.ToString(),
                            AddedBy = activeGroup.AddedBy
                        });
                    }
                    else
                    {
                        response.ServiceRuleDetails.Add(new ServiceRuleDetails
                        {
                            SubClientName = subClient.Name
                        });
                    }
                }
                else
                {
                    response.ServiceRuleDetails.Add(new ServiceRuleDetails
                    {
                        SubClientName = subClient.Name
                    });
                }
            }

            return response;
        }

        public async Task<GetBinDetailsResponse> GetBinDetails()
        {
            var response = new GetBinDetailsResponse();

            var sites = await siteProcessor.GetAllSitesAsync();

            foreach (var site in sites)
            {
                var currentActiveGroupId = await GetBinActiveGroupIdAsync(site.SiteName);

                if (StringHelper.Exists(currentActiveGroupId))
                {
                    var activeGroup = await GetActiveGroupByIdAsync(currentActiveGroupId, site.SiteName);

                    if (StringHelper.Exists(activeGroup.Id))
                    {
                        response.BinDetails.Add(new BinDetails
                        {
                            Name = site.SiteName,
                            ActiveGroupId = activeGroup.Id,
                            StartDate = activeGroup.StartDate.ToString(),
                            CreateDate = activeGroup.CreateDate.ToString(),
                            AddedBy = activeGroup.AddedBy
                        });
                    }
                    else
                    {
                        response.BinDetails.Add(new BinDetails
                        {
                            Name = site.SiteName
                        });
                    }
                }
                else
                {
                    response.BinDetails.Add(new BinDetails
                    {
                        Name = site.SiteName
                    });
                }
            }

            return response;
        }

        public async Task<ActiveGroup> GetCurrentActiveGroup(string key, string name)
        {
            return await activeGroupRepository.GetCurrentActiveGroup(key, name);
        }

        public void Update(ActiveGroup result)
        {
			activeGroupRepository.UpdateItemAsync(result);
        }

        public async Task<BatchDbResponse<ActiveGroup>> UpdateSetDatasetProcessed(List<ActiveGroup> activeGroups)
		{
			return await activeGroupRepository.UpdateSetDatasetProcessed(activeGroups);
        }
    }
}
