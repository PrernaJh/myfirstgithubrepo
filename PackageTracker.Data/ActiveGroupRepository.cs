using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.CosmosDb;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTracker.Data
{
    public class ActiveGroupRepository : CosmosDbRepository<ActiveGroup>, IActiveGroupRepository
    {
        private readonly ILogger<ActiveGroupRepository> _logger;
        public ActiveGroupRepository(ILogger<ActiveGroupRepository> logger, IConfiguration configuration, ICosmosDbContainerFactory factory) :
            base(logger, configuration, factory)
        {
            _logger = logger;

        }

        public override string ContainerName { get; } = CollectionNameConstants.ActiveGroups;

        public override string ResolvePartitionKeyString(string input = null) => PartitionKeyUtility.GeneratePartitionKeyLiteralString(input);

        public async Task<string> GetCurrentActiveGroupIdAsync(string activeGroupType, string name, DateTime localDateTime)
        {
            var query = $@"SELECT TOP 1 ag.id FROM {ContainerName} ag WHERE ag.name = @name
                            AND ag.activeGroupType = @activeGroupType
                            AND ag.startDate < @localDateTime
                            AND ag.isEnabled = true
                            ORDER BY ag.startDate DESC, ag.createDate DESC";            
            var queryDefinition = new QueryDefinition(query)
                                                    .WithParameter("@name", name)
                                                    .WithParameter("@activeGroupType", activeGroupType)
                                                    .WithParameter("@localDateTime", localDateTime);
            var results = await GetItemsAsync(queryDefinition, name);
            var response = results.FirstOrDefault() ?? new ActiveGroup();
            return response.Id;
        }

        public async Task<ActiveGroup> GetCurrentActiveGroup(string key, string name)
        {
            var query = $@"SELECT  * FROM {ContainerName} ag WHERE ag.id = @id";
            var queryDefinition = new QueryDefinition(query)
                                                    .WithParameter("@id", key);

            var results = await GetItemsAsync(queryDefinition, name);
            return results.FirstOrDefault() ?? new ActiveGroup();
        }

        public async Task<IEnumerable<ActiveGroup>> GetCurrentActiveGroupsWithEndDateAsync(string activeGroupType, string name, DateTime localDateTime)
        {
            var query = $@"SELECT * FROM {ContainerName} ag WHERE ag.name = @name
                            AND ag.activeGroupType = @activeGroupType
                            AND ag.startDate < @localDateTime
                            AND ag.endDate > @localDateTime
                            AND ag.isEnabled = true
                            ORDER BY ag.startDate DESC, ag.createDate DESC";
            var queryDefinition = new QueryDefinition(query)
                                                    .WithParameter("@name", name)
                                                    .WithParameter("@activeGroupType", activeGroupType)
                                                    .WithParameter("@localDateTime", localDateTime);
            var results = await GetItemsAsync(queryDefinition, name);
            return results;
        }

        public async Task<IEnumerable<ActiveGroup>> GetActiveGroupsByTypeAsync(string activeGroupType, string name)
        {
            var query = $@"SELECT * FROM {ContainerName} ag 
                            WHERE ag.name = @name
                                AND ag.activeGroupType = @activeGroupType";
            var queryDefinition = new QueryDefinition(query)
                                                    .WithParameter("@name", name)
                                                    .WithParameter("@activeGroupType", activeGroupType);
            var results = await GetItemsAsync(queryDefinition, name);
            return results;
        }

        public async Task<bool> HaveActiveGroupsChangedAsync(string activeGroupType, string name, DateTime lastScanDateTime)
        {
            if (lastScanDateTime.Year == 1)
                return true;
            var startOfUnixEpoch = new DateTime(1970, 1, 1); // Jan 1, 1970
            var unixTimeAtLastScan = ((int)(lastScanDateTime - startOfUnixEpoch).TotalSeconds) - 1;
            var query = $@"SELECT TOP 1 ag._ts, ag.name, ag.activeGroupType FROM {ContainerName} ag
									WHERE ag.name = @name
                                        AND ag.activeGroupType = @activeGroupType
										AND ag._ts >= @unixTimeAtLastScan";
            var queryDefinition = new QueryDefinition(query)
                                                    .WithParameter("@name", name)
                                                    .WithParameter("@activeGroupType", activeGroupType)
                                                    .WithParameter("@unixTimeAtLastScan", unixTimeAtLastScan);
           var results = await this.GetTimestampsAsync(queryDefinition);
           return results.Any();
        }
    }
}
