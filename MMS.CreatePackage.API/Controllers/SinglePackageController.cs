using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.CreatePackage;
using MMS.API.Domain.Utilities;
using PackageTracker.AzureExtensions;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace MailManifestSystem.CreatePackageAPI
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SinglePackageController : ControllerBase
    {
        private readonly ILogger<SinglePackageController> logger;
        private readonly ICreatePackageProcessor packageProcessor;
        private readonly IServiceBusHelper serviceBusHelper;

        public SinglePackageController(ILogger<SinglePackageController> logger,
            ICreatePackageProcessor packageProcessor,
            IServiceBusHelper serviceBusHelper)
        {
            this.logger = logger;
            this.packageProcessor = packageProcessor;
            this.serviceBusHelper = serviceBusHelper;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreatePackageResponse), 200)]
        public async Task<IActionResult> CreatePackage(CreatePackageRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"CreatePackage: {JsonSerializer.Serialize(request)}");

                var response = await packageProcessor.ProcessCreatePackageRequestAsync(request);
                var serializedResponse = JsonSerializer.Serialize(response.CreatePackageResponse);
                logger.LogInformation($"CreatePackageResponse: {serializedResponse} Timer data: {LogUtility.GenerateCreatePackageTimerMessage(response.Timer)}");

                // service bus features are disabled for now, we may enable in the future
                //try
                //{
                //	var serviceBusMessage = await serviceBusHelper.SendTopicMessageAsync(response.ServiceBusPayload);
                //	logger.LogInformation($"Service Bus Message Sent: {serviceBusMessage}");
                //}
                //catch (Exception ex)
                //{
                //	logger.LogError($"Service Bus Write Exception: {ex}");
                //}

                return Ok(response.CreatePackageResponse);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to create package. Request: {JsonSerializer.Serialize(request)} Exception: {ex}");
                return BadRequest($"Failed to create package");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DeletePackageResponse), 200)]
        public async Task<IActionResult> DeletePackage(DeletePackageRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"DeletePackage: {JsonSerializer.Serialize(request)}");

                var response = await packageProcessor.DeletePackageAsync(request);
                var serializedResponse = JsonSerializer.Serialize(response);
                logger.LogInformation($"DeletePackageResponse: {serializedResponse}");

                //try
                //{
                //	var serviceBusMessage = await serviceBusHelper.SendTopicMessageAsync(response.ServiceBusPayload);
                //	logger.LogInformation($"Service Bus Message Sent: {serviceBusMessage}");
                //}
                //catch (Exception ex)
                //{
                //	logger.LogError($"Service Bus Write Exception: {ex}");
                //}

                return Ok(response);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to delete package. Request: {JsonSerializer.Serialize(request)} Exception: {ex}");
                return BadRequest($"Failed to delete package");
            }
        }
    }
}
