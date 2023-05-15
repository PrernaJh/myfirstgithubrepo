using PackageTracker.Data.Constants;
using System.Collections.Generic;

namespace PackageTracker.CosmosDbExtensions
{
	public static class CollectionNamesList
	{
		public static List<string> CollectionNames = new List<string>
		{
			CollectionNameConstants.ActiveGroups,
			CollectionNameConstants.BinMaps,
			CollectionNameConstants.Bins,
			CollectionNameConstants.Clients,
			CollectionNameConstants.ClientFacilities,
			CollectionNameConstants.Containers,
			CollectionNameConstants.FileConfigurations,
			CollectionNameConstants.Jobs,
			CollectionNameConstants.JobOptions,
			CollectionNameConstants.OperationalContainers,
			CollectionNameConstants.Packages,
			CollectionNameConstants.Rates,
			CollectionNameConstants.ReturnOptions,
			CollectionNameConstants.Settings,
			CollectionNameConstants.ServiceRules,
			CollectionNameConstants.ServiceRuleExtensions,
			CollectionNameConstants.Sequences,
			CollectionNameConstants.Sites,
			CollectionNameConstants.SubClients,
			CollectionNameConstants.WebJobRuns,
			CollectionNameConstants.ZipMaps,
			CollectionNameConstants.ZipOverrides,
			CollectionNameConstants.ZoneMaps
		};
	}
}
