using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Models.ActiveGroupDetails;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Interfaces
{
	public interface IActiveGroupProcessor
	{
		Task<DateTime> GetLocalTimeBySite(string siteName, string timeZone = null);
		Task<DateTime> GetLocalTimeBySubClient(string subClientName, string timeZone = null);

		Task<ActiveGroup> GetActiveGroupByIdAsync(string id, string name);
		// add
		Task<ActiveGroup> AddActiveGroupAsync(ActiveGroup activeGroup);
		
		// by site
		Task<string> GetBinActiveGroupIdAsync(string siteName, string siteTimeZone = null);
		Task<string> GetBinActiveGroupIdByDateAsync(string siteName, DateTime? dateTime, string timeZone);
		Task<string> GetUpsGeoDescActiveGroupId(string siteName, string siteTimeZone = null);

		Task<string> GetContainerRatesActiveGroupIdAsync(string siteName, string timeZone = null);
		Task<IEnumerable<ActiveGroup>> GetContainerRatesActiveGroupsAsync(string siteName);

		// by subclient
		Task<string> GetRatesActiveGroupIdAsync(string subClientName, string subClientTimeZone = null);
		Task<string> GetServiceRuleActiveGroupIdAsync(string subClientName, string subClientTimeZone = null);
		Task<string> GetServiceRuleActiveGroupIdByDateAsync(string subClientName, DateTime? dateTime, string timeZone);		
		Task<List<ActiveGroup>> GetServiceOverrideActiveGroupsAsync(string subClientName, string packageTimeZone);
		Task<string> GetBinMapActiveGroupIdAsync(string subClientName, string subClientTimeZone = null);
		Task<string> GetBinMapActiveGroupIdByDateAsync(string siteName, DateTime? dateTime, string timeZone);
		Task<string> GetZipCarrierOverrideActiveGroupIdAsync(string subClientName, string subClientTimeZone = null);

		// by client
		Task<string> GetFortyEightStatesActiveGroupIdAsync(string clientName);

		// global		
		Task<string> GetZipMapActiveGroup(string activeGroupType);
		Task<string> GetZoneMapActiveGroupIdAsync();
		Task<List<string>> GetZipOverrideActiveGroupIds(string subClientName);

		// lists for views
		Task<IEnumerable<ActiveGroup>> GetShippingMethodOverrideActiveGroupsAsync(string subClientName);
		Task<IEnumerable<ActiveGroup>> GetZipOverrideActiveGroupsAsync(string subClientName);
		Task<IEnumerable<ActiveGroup>> GetAllRateActiveGroupsAsync(string subClientName);
		Task<IEnumerable<ActiveGroup>> GetAllActiveGroupsByType(string activeGroupType, bool isGlobal, string name = null);
		Task<IEnumerable<ActiveGroup>> GetServiceRuleActiveGroupsAsync(string subClientName);
		Task<IEnumerable<ActiveGroup>> GetFortyEightStatesActiveGroupsAsync(string clientName);
		Task<IEnumerable<ActiveGroup>> GetBinSchemaActiveGroupsAsync(string siteName);
		Task<IEnumerable<ActiveGroup>> GetBinMapsActiveGroupsAsync(string subClient);

		// get active group upload details
		Task<GetServiceRuleDetailsResponse> GetServiceRuleDetails();
		Task<GetBinDetailsResponse> GetBinDetails();
        Task<ActiveGroup> GetCurrentActiveGroup(string key, string name);
        void Update(ActiveGroup result);
        Task<BatchDbResponse<ActiveGroup>> UpdateSetDatasetProcessed(List<ActiveGroup> activeGroups);
    }
}
