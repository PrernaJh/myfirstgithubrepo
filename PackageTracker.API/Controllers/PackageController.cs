using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models;
using MMS.API.Domain.Utilities;
using PackageTracker.Identity.Data.Constants;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace PackageTracker.API.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly IPackageScanProcessor packageProcessor;
        private readonly ILogger<PackageController> logger;

        public PackageController(IPackageScanProcessor packageProcessor, ILogger<PackageController> logger)
        {
            this.packageProcessor = packageProcessor;
            this.logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ScanPackageResponse), 200)]
        public async Task<IActionResult> ScanPackage(ScanPackageRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, LogUtility.GenerateScanPackageRequestLog(request));

                var response = await packageProcessor.ProcessScanPackageRequest(request);

                logger.Log(LogLevel.Information, $"ScanPackageResponse: {LogUtility.GenerateScanPackageResponseLog(response.ScanPackageResponse)}");
                logger.Log(LogLevel.Information, $"ScanPackage Timer: {LogUtility.GenerateScanPackageTimerMessage(response.Timer)}");

                return Ok(JsonSerializer.Serialize(response.ScanPackageResponse));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "ScanPackage Exception: " + ex);
                return Ok(JsonSerializer.Serialize(new ScanPackageResponse()));
            }
        }

        [HttpPost]
        [Authorize(Roles = IdentityDataConstants.GMAdminSuperQA)]
        [ProducesResponseType(typeof(ScanPackageResponse), 200)]
        public async Task<IActionResult> RepeatScanPackage(ScanPackageRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, LogUtility.GenerateScanPackageRequestLog(request));

                var isRepeatScan = true;
                var response = await packageProcessor.ProcessScanPackageRequest(request, isRepeatScan);

                logger.Log(LogLevel.Information, $"RepeatScanPackageResponse: {LogUtility.GenerateScanPackageResponseLog(response.ScanPackageResponse)}");
                logger.Log(LogLevel.Information, $"RepeatScanPackage Timer: {LogUtility.GenerateScanPackageTimerMessage(response.Timer)}");

                return Ok(JsonSerializer.Serialize(response.ScanPackageResponse));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "RepeatScanPackage Exception: " + ex);
                return Ok(JsonSerializer.Serialize(new ScanPackageResponse()));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReprintPackageResponse), 200)]
        [Authorize(Roles = IdentityDataConstants.GMAdminSuperQA)]
        [ProducesResponseType(typeof(ReprintPackageResponse), 200)]
        public async Task<IActionResult> ReprintPackage(ReprintPackageRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"ReprintPackage Request: {JsonSerializer.Serialize(request)}");
                var response = await packageProcessor.ReprintPackageAsync(request);
                logger.Log(LogLevel.Information, $"ReprintPackage Response: {JsonSerializer.Serialize(response)}");
                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"ReprintPackage Exception: {ex}");
                return Ok(JsonSerializer.Serialize(new ReprintPackageResponse()));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ValidatePackageResponse), 200)]
        public async Task<IActionResult> ValidatePackage(ValidatePackageRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, LogUtility.GenerateValidatePackageRequestLog(request));
                var timer = Stopwatch.StartNew();
                var response = await packageProcessor.ProcessValidatePackageRequest(request);
                timer.Stop();

                response.Message = LogUtility.GenerateValidatePackageTimerMessage(timer);
                logger.Log(LogLevel.Information, $"ValidatePackage Response: {LogUtility.GenerateValidatePackageResponseLog(response)}");
                logger.Log(LogLevel.Information, $"ValidatePackage Timer: {LogUtility.GenerateValidatePackageTimerMessage(timer)}");
                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "ValidatePackage Exception: " + ex);
                return Ok(JsonSerializer.Serialize(new ValidatePackageResponse()));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(GetPackageHistoryResponse), 200)]
        [Authorize(Roles = IdentityDataConstants.GMAdminSuperQA)]
        [ProducesResponseType(typeof(GetPackageHistoryResponse), 200)]
        public async Task<IActionResult> GetPackageHistory(GetPackageHistoryRequest request)
        {
            try
            {
                var response = await packageProcessor.GetPackageEvents(request);
                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"GetPackageHistory Exception: {ex}");
                return Ok(JsonSerializer.Serialize(new ReprintPackageResponse()));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ForceExceptionResponse), 200)]
        public async Task<IActionResult> ForceException(ForceExceptionRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, JsonSerializer.Serialize(request));

                var response = await packageProcessor.ProcessForceExceptionRequest(request);

                logger.Log(LogLevel.Information, JsonSerializer.Serialize(response));

                return Ok(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, "ForceException Exception: " + ex);
                return Ok(JsonSerializer.Serialize(new ScanPackageResponse()));
            }
        }
    }
}