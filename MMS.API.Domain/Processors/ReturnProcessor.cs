using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.Returns;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Utilities;
using System;
using System.Threading.Tasks;

namespace MMS.API.Domain.Processors
{
	public class ReturnProcessor : IReturnProcessor
	{
		private readonly IEodPostProcessor eodPostProcessor;
		private readonly ILogger<ReturnProcessor> logger;
		private readonly IPackageLabelProcessor labelProcessor;
		private readonly IPackageRepository packageRepository;
		private readonly IReturnOptionRepository returnOptionRepository;
		private readonly IShippingProcessor shippingProcessor;
		private readonly ISubClientProcessor subClientProcessor;

		public ReturnProcessor(IEodPostProcessor eodPostProcessor,
			ILogger<ReturnProcessor> logger,
			IPackageLabelProcessor labelProcessor,
			IPackageRepository packageRepository,
			IReturnOptionRepository returnOptionRepository,
			IShippingProcessor shippingProcessor,
			ISubClientProcessor subClientProcessor)
		{
			this.eodPostProcessor = eodPostProcessor;
			this.logger = logger;
			this.labelProcessor = labelProcessor;
			this.packageRepository = packageRepository;
			this.returnOptionRepository = returnOptionRepository;
			this.shippingProcessor = shippingProcessor;
			this.subClientProcessor = subClientProcessor;
		}

		public async Task<GetReturnOptionResponse> GetReturnOptionsAsync(string siteName)
		{
			var response = new GetReturnOptionResponse();
			var returnOption = await returnOptionRepository.GetReturnOptionsBySiteAsync(siteName);
			logger.LogInformation("return option id: " + returnOption.Id);

			response.ReturnReasons.AddRange(returnOption.ReturnReasons);
			response.ReasonDescriptions.AddRange(returnOption.ReasonDescriptions);

			return response;
		}

		public async Task<ReturnPackageResponse> ReturnPackageAsync(ReturnPackageRequest request)
		{
			try
			{
				var response = new ReturnPackageResponse();
				var package = await packageRepository.GetReturnPackage(request.PackageId, request.SiteName);

				if (StringHelper.Exists(package.Id) && !package.IsCreated)
				{
					package.IsLocked = await eodPostProcessor.ShouldLockPackage(package);

					if (!package.IsLocked)
					{
						var returnLabelRequest = new ReturnLabelRequest
						{
							SiteName = request.SiteName,
							PackageId = request.PackageId,
							ReturnReason = request.ReturnReasonValue,
							ReturnDescription = request.ReasonDescriptionValue
						};
						package.PackageStatus = EventConstants.Exception;
						package.EodUpdateCounter += 1;
						package.LabelTypeId = LabelTypeIdConstants.ReturnToSender;
						package.ReturnLabelFieldValues = labelProcessor.GetLabelFieldsForReturnToCustomer(returnLabelRequest);

						package.PackageEvents.Add(new Event
						{
							EventId = package.PackageEvents.Count + 1,
							EventType = EventConstants.ManualReturn,
							EventStatus = EventConstants.Exception,
							Description = request.ReturnReasonValue,
							Username = request.Username,
							MachineId = request.MachineId,
							EventDate = DateTime.Now,
							LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
						});

						if (package.ShippingCarrier == ShippingCarrierConstants.Ups)
						{
							var subClient = await subClientProcessor.GetSubClientByNameAsync(package.SubClientName);
							await shippingProcessor.VoidUpsShipmentAsync(package, subClient);
						}

						response = new ReturnPackageResponse
						{
							LabelTypeId = package.LabelTypeId,
							LabelFieldValues = package.ReturnLabelFieldValues
						};
					}
					else
					{
						package.PackageEvents.Add(new Event
						{
							EventId = package.PackageEvents.Count + 1,
							EventType = EventConstants.ManualReturn,
							EventStatus = package.PackageStatus,
							Description = "Package locked by end of day file process",
							Username = request.Username,
							MachineId = request.MachineId,
							EventDate = DateTime.Now,
							LocalEventDate = TimeZoneUtility.GetLocalTime(package.TimeZone)
						});

						response = new ReturnPackageResponse
						{
							LabelTypeId = LabelTypeIdConstants.ReturnToSender,
							LabelFieldValues = labelProcessor.GetLabelFieldsForReturnEodProcessed(package.SiteName, package.PackageId)
						};
					};

					await packageRepository.UpdateItemAsync(package);
					return response;
				}
				else
				{
					logger.LogInformation($"Failed to Return PackageId: {request.PackageId} Site {request.SiteName} PackageId not found");
					return new ReturnPackageResponse();
				}
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Error while returning package label. PackageId: {request.PackageId} Username: {request.Username}. Exception: {ex}");
				return new ReturnPackageResponse();
			}
		}
	}
}
