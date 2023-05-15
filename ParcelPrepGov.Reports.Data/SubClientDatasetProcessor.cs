using Microsoft.Extensions.Logging;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Data
{
	public class SubClientDatasetProcessor : ISubClientDatasetProcessor
    {
		private readonly ILogger<SubClientDatasetProcessor> logger;

		private readonly ISubClientDatasetRepository subClientDatasetRepository;
		private readonly ISubClientProcessor subClientProcessor;

		public SubClientDatasetProcessor(ILogger<SubClientDatasetProcessor> logger,
			ISubClientDatasetRepository subClientDatasetRepository,
			ISubClientProcessor subClientProcessor
			)
        {
			this.logger = logger;

			this.subClientProcessor = subClientProcessor;
			this.subClientDatasetRepository = subClientDatasetRepository;
        }

        public async Task<ReportResponse> UpdateSubClientDatasets()
        {
			var response = new ReportResponse() { IsSuccessful = true };
			try
			{
				var subClientsToUpdate = (await subClientProcessor.GetSubClientsAsync())
					.Where(x => x.IsDatasetProcessed == false);
				var subClientDatasets = new List<SubClientDataset>();
				foreach (var subClient in subClientsToUpdate)
				{
					CreateDataset(subClientDatasets, subClient);
				}
				if (subClientDatasets.Any() || subClientsToUpdate.Any())
				{
					logger.LogInformation($"Number of subClient datasets to insert/update: {subClientDatasets.Count()}");
					await BulkInsertAndUpdate(response, subClientDatasets, subClientsToUpdate.ToList());
				}
			}
			catch (Exception ex)
			{
				response.Message = ex.ToString();
				response.IsSuccessful = false;
				logger.LogError($"Failed to bulk insert/update subClient datasets. Exception: { ex }");
			}
			return response;
		}

		private async Task BulkInsertAndUpdate(ReportResponse response,
			List<SubClientDataset> subClientDatasets, List<SubClient> subClientsToUpdate)
		{
			response.NumberOfDocuments = subClientDatasets.Count();
			response.IsSuccessful = await subClientDatasetRepository.ExecuteBulkInsertOrUpdateAsync(subClientDatasets);
			if (response.IsSuccessful)
            {
				subClientsToUpdate.ForEach(x => x.IsDatasetProcessed = true);
				var bulkUpdate = await subClientProcessor.UpdateSetDatasetProcessed(subClientsToUpdate);
				if (! bulkUpdate.IsSuccessful)
                {
					response.IsSuccessful = false;
					response.Message = bulkUpdate.Message;
                }
			}
			else
            {
				response.Message = "SubClientDatasetRepository.ExecuteBulkInsertOrUpdateAsync failed";
            }
		}

		public void CreateDataset(List<SubClientDataset> subClientDatasets, SubClient subClient)
		{
			subClientDatasets.Add(new SubClientDataset()
			{
				CosmosId = subClient.Id,
				CosmosCreateDate = subClient.CreateDate,
				SiteName = subClient.SiteName,
				ClientName = subClient.ClientName,
				Name = subClient.Name,
				Description = subClient.Description,
				Key = subClient.Key
			});
		}
	}
}
