using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.Containers;
using PackageTracker.Domain.Models;
using PackageTracker.Identity.Data.Constants;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PackageTracker.API.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ContainerController : ControllerBase
    {
        private readonly IContainerProcessor containerProcessor;
        private readonly IContainerUpdateProcessor containerScanProcessor;
        private readonly ILogger<ContainerController> logger;
        private readonly IPackageContainerProcessor packageContainerProcessor;

        public ContainerController(
            IContainerProcessor containerProcessor,
            IContainerUpdateProcessor containerScanProcessor,
            ILogger<ContainerController> logger,
            IPackageContainerProcessor packageContainerProcessor)
        {
            this.containerProcessor = containerProcessor;
            this.containerScanProcessor = containerScanProcessor;
            this.logger = logger;
            this.packageContainerProcessor = packageContainerProcessor;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetBinCodesResponse), 200)]
        public async Task<IActionResult> GetBinCodes(string siteName)
        {
            try
            {
                var response = await containerProcessor.GetBinCodesAsync(siteName);

                var serializedResponse = JsonSerializer.Serialize(response);

                logger.Log(LogLevel.Information, $"GetBinCodesResponse: {serializedResponse}");

                return Ok(serializedResponse);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to get bin codes. Exception: {ex}");
                return BadRequest($"Failed to get bin codes");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateContainersResponse), 200)]
        public async Task<IActionResult> CreateContainers(CreateContainersRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"CreateContainersRequest: {JsonSerializer.Serialize(request)}");

                var response = await containerProcessor.CreateContainersAsync(request);
                var serializedResponse = JsonSerializer.Serialize(response);

                logger.Log(LogLevel.Information, $"CreateContainersResponse: {serializedResponse}");

                return Ok(serializedResponse);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to create containers. Request: {JsonSerializer.Serialize(request)} Exception: {ex}");
                return BadRequest($"Failed to create containers");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(CloseContainerResponse), 200)]
        public async Task<IActionResult> CloseContainer(CloseContainerRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"ScanContainerRequest: {JsonSerializer.Serialize(request)}");
                var response = await containerScanProcessor.CloseContainerAsync(request);
                var serializedResponse = JsonSerializer.Serialize(response);
                logger.Log(LogLevel.Information, $"ScanContainerResponse: {serializedResponse}");

                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to scan container. Exception: {ex}");
                return BadRequest($"Failed to scan container");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DeleteContainerResponse), 200)]
        public async Task<IActionResult> DeleteContainer(DeleteContainerRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"DeleteContainerRequest: {JsonSerializer.Serialize(request)}");
                var response = await containerScanProcessor.DeleteContainerAsync(request);
                var serializedResponse = JsonSerializer.Serialize(response);
                logger.Log(LogLevel.Information, $"DeleteContainerRequest: {serializedResponse}");

                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to delete container. Exception: {ex}");
                return BadRequest($"Failed to delete container");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(UpdateContainerResponse), 200)]
        public async Task<IActionResult> UpdateContainer(UpdateContainerRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"UpdateContainerRequest: {JsonSerializer.Serialize(request)}");
                var response = await containerScanProcessor.UpdateContainerAsync(request);
                var serializedResponse = JsonSerializer.Serialize(response);
                logger.Log(LogLevel.Information, $"UpdateContainerResponse: {serializedResponse}");

                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to update container. Exception: {ex}");
                return BadRequest($"Failed to update container");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReplaceContainerResponse), 200)]
        public async Task<IActionResult> ReplaceContainer(ReplaceContainerRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"ReplaceContainerRequest: {JsonSerializer.Serialize(request)}");
                var response = await containerScanProcessor.ReplaceContainerAsync(request);
                var serializedResponse = JsonSerializer.Serialize(response);
                logger.Log(LogLevel.Information, $"ReplaceContainerResponse: {serializedResponse}");

                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to replace container. Exception: {ex}");
                return BadRequest($"Failed to replace container");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReprintClosedContainerResponse), 200)]
        [Authorize(Roles = IdentityDataConstants.GMAdminSuperQA)]
        public async Task<IActionResult> ReprintClosedContainer(ReprintClosedContainerRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"ReprintClosedContainerRequest: {JsonSerializer.Serialize(request)}");
                var response = await containerProcessor.ReprintClosedContainerAsync(request);
                var serializedResponse = JsonSerializer.Serialize(response);
                logger.Log(LogLevel.Information, $"ReprintClosedContainerResponse: {serializedResponse}");

                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to reprint closed container. Exception: {ex}");
                return BadRequest($"Failed to reprint closed container");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReprintActiveContainersResponse), 200)]
        [Authorize(Roles = IdentityDataConstants.GMAdminSuperQA)]
        public async Task<IActionResult> ReprintActiveContainers(ReprintActiveContainersRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"ReprintActiveContainersRequest: {JsonSerializer.Serialize(request)}");
                var response = await containerProcessor.ReprintActiveContainersAsync(request);
                var serializedResponse = JsonSerializer.Serialize(response);
                logger.Log(LogLevel.Information, $"ReprintActiveContainersResponse: {serializedResponse}");

                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to reprint active containers. Exception: {ex}");
                return BadRequest($"Failed to reprint active containers");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(AssignContainerResponse), 200)]
        public async Task<IActionResult> AssignNewContainer(AssignContainerRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"Assign New Container Request: {JsonSerializer.Serialize(request)}");
                var response = await packageContainerProcessor.AssignPackageNewContainerAsync(request);
                var serializedResponse = JsonSerializer.Serialize(response);
                logger.Log(LogLevel.Information, $"Assign New Container Response: {serializedResponse}");

                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to assign active container. Exception: {ex}");
                return BadRequest($"Failed to assign active container");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(AssignContainerResponse), 200)]
        public async Task<IActionResult> AssignActiveContainer(AssignContainerRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"Assign Active Container Request: {JsonSerializer.Serialize(request)}");
                var response = await packageContainerProcessor.AssignPackageActiveContainerAsync(request);
                var serializedResponse = JsonSerializer.Serialize(response);
                logger.Log(LogLevel.Information, $"Assign Active Container Response: {serializedResponse}");

                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to assign active container. Exception: {ex}");
                return BadRequest($"Failed to assign active container");
            }
        }
    }
}