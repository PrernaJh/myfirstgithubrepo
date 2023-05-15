using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Data
{
	public class BinDatasetProcessor : IBinDatasetProcessor
	{
		private readonly ILogger<BinDatasetProcessor> logger;
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly IBinDatasetRepository binDatasetRepository;
		private readonly IBinProcessor binProcessor;

		public BinDatasetProcessor(ILogger<BinDatasetProcessor> logger,
			IActiveGroupProcessor activeGroupProcessor,
			IBinDatasetRepository binDatasetRepository,
			IBinProcessor binProcessor
			)
		{
			this.logger = logger;
			this.activeGroupProcessor = activeGroupProcessor;
			this.binDatasetRepository = binDatasetRepository;
			this.binProcessor = binProcessor;
		}

		public async Task<ReportResponse> UpdateBinDatasets(Site site)
		{
			var response = new ReportResponse() { IsSuccessful = true };
			try
			{
				var binActiveGroups = 
					(await activeGroupProcessor.GetAllActiveGroupsByType(ActiveGroupTypeConstants.Bins, false, site.SiteName))
					.Where(x => x.IsDatasetProcessed == false);
				var activeGroupsToUpdate = new List<ActiveGroup>();
				var binDatasets = new List<BinDataset>();
				foreach (var activeGroup in binActiveGroups)
                {
					var bins = await binProcessor.GetBinsByActiveGroupIdAsync(activeGroup.Id);
					foreach (var bin in bins)
					{
						CreateDataset(binDatasets, bin, activeGroup);
					}
					activeGroupsToUpdate.Add(activeGroup);
                }
				if (binDatasets.Any() || activeGroupsToUpdate.Any())
				{
					logger.LogInformation($"Number of bin datasets to insert: {binDatasets.Count()}");
					await BulkInsertAndUpdate(site, response, binDatasets, activeGroupsToUpdate);
				}
			}
			catch (Exception ex)
			{
				response.Message = ex.ToString();
				response.IsSuccessful = false;
				logger.LogError($"Failed to bulk insert bin datasets for site: { site.SiteName }. Exception: { ex }");
			}
			return response;
		}

		private async Task BulkInsertAndUpdate(Site site, ReportResponse response,
			List<BinDataset> binDatasets, List<ActiveGroup> activeGroupsToUpdate)
		{
			response.NumberOfDocuments = binDatasets.Count();
			var bulkInsert = await binDatasetRepository.ExecuteBulkInsertAsync(binDatasets, site.SiteName);
			if (bulkInsert)
			{
				activeGroupsToUpdate.ForEach(x => x.IsDatasetProcessed = true);
				var bulkUpdate = await activeGroupProcessor.UpdateSetDatasetProcessed(activeGroupsToUpdate);
				if (! bulkUpdate.IsSuccessful)
				{
					response.NumberOfFailedDocuments = bulkUpdate.FailedCount;
					response.IsSuccessful = false;
					logger.LogError($"Failed to bulk update bin active groups for site: { site.SiteName }. Failures: {response.NumberOfFailedDocuments} Total Expected: {activeGroupsToUpdate.Count()}");
				}
			}
			else
			{
				response.NumberOfFailedDocuments = binDatasets.Count();
				response.IsSuccessful = false;
				logger.LogError($"Failed to bulk insert bin datasets for site: { site.SiteName }. Total Failures: {response.NumberOfFailedDocuments}");
			}
		}

		public void CreateDataset(List<BinDataset> binDatasets, Bin bin, ActiveGroup activeGroup)
		{
			if (StringHelper.Exists(bin.LabelListZip) && !Regex.IsMatch(bin.LabelListZip, @"^[0-9-]+$"))
				return; // Bad data in zip from historical bin imports.
			if (StringHelper.DoesNotExist(bin?.BinCode))
				return;
			binDatasets.Add(new BinDataset
			{
				CosmosId = bin.Id,
				CosmosCreateDate = bin.CreateDate,
				ActiveGroupId = bin.ActiveGroupId,
				BinCode = bin.BinCode,

				LabelListSiteKey = bin.LabelListSiteKey,
				LabelListDescription = bin.LabelListDescription,
				LabelListZip = bin.LabelListZip,

				OriginPointSiteKey = bin.OriginPointSiteKey,
				OriginPointDescription = bin.OriginPointDescription,

				DropShipSiteKeyPrimary = bin.DropShipSiteKeyPrimary,
				DropShipSiteDescriptionPrimary = bin.DropShipSiteDescriptionPrimary,
				DropShipSiteAddressPrimary = bin.DropShipSiteAddressPrimary,
				DropShipSiteCszPrimary = bin.DropShipSiteCszPrimary,
				ShippingCarrierPrimary = bin.ShippingCarrierPrimary,
				ShippingMethodPrimary = bin.ShippingMethodPrimary,
				ContainerTypePrimary = bin.ContainerTypePrimary,
				LabelTypePrimary = bin.LabelTypePrimary,
				RegionalCarrierHubPrimary = bin.RegionalCarrierHubPrimary,
				DaysOfTheWeekPrimary = bin.DaysOfTheWeekPrimary,
				ScacPrimary = bin.ScacPrimary,
				AccountIdPrimary = bin.AccountIdPrimary,

				BinCodeSecondary = bin.BinCodeSecondary,
				DropShipSiteKeySecondary = bin.DropShipSiteKeySecondary,
				DropShipSiteDescriptionSecondary = bin.DropShipSiteDescriptionSecondary,
				DropShipSiteAddressSecondary = bin.DropShipSiteAddressSecondary,
				DropShipSiteCszSecondary = bin.DropShipSiteCszSecondary,
				ShippingMethodSecondary = bin.ShippingMethodSecondary,
				ContainerTypeSecondary = bin.ContainerTypeSecondary,
				LabelTypeSecondary = bin.LabelTypeSecondary,
				RegionalCarrierHubSecondary = bin.RegionalCarrierHubSecondary,
				DaysOfTheWeekSecondary = bin.DaysOfTheWeekSecondary,
				ScacSecondary = bin.ScacSecondary,
				AccountIdSecondary = bin.AccountIdSecondary,
				ShippingCarrierSecondary = bin.ShippingCarrierSecondary,

				IsAptb = bin.IsAptb,
				IsScsc = bin.IsScsc
			});
		}
	}
}
