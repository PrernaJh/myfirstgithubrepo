using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models.AutoScan;
using MMS.API.Domain.Utilities;
using PackageTracker.Domain.Models;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data.Models;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PackageTracker.API.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AutoScanController : ControllerBase
    {

        private readonly IAutoScanProcessor autoScanProcessor;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AutoScanController> logger;
        private readonly IConfiguration config;

        public AutoScanController(ILogger<AutoScanController> logger, UserManager<ApplicationUser> userManager, IAutoScanProcessor autoScanProcessor, IConfiguration config)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(ILogger<ContainerController>));
            this.autoScanProcessor = autoScanProcessor ?? throw new ArgumentException(nameof(autoScanProcessor));
            this.userManager = userManager ?? throw new ArgumentException(nameof(userManager));
            this.config = config ?? throw new ArgumentException(nameof(config));
        }

        [HttpPost(Name = nameof(GetParcelData))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ParcelDataResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<GenericResponse<ParcelDataResponse>>> GetParcelData(ParcelDataRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"{JsonSerializer.Serialize(request)}");
                var responseAndTimer = await autoScanProcessor.ProcessAutoScanPackageRequest(request);

                var genericResponse = new GenericResponse<ParcelDataResponse>(responseAndTimer.Response);
                logger.Log(LogLevel.Information, $"ScanPackage Timer: {LogUtility.GenerateScanPackageTimerMessage(responseAndTimer.Timer)}");
                logger.Log(LogLevel.Information, $"{JsonSerializer.Serialize(genericResponse)}");
                return Ok(genericResponse);
            }

            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"GetParcelData Error for Site: {request.Site} Job: {request.JobId} Package ID: {request.Barcode} machine ID: {request.MachineId} Exception: {ex}");
                var errorTemplate = config.GetSection("ZPLSettings").GetSection("AutoScanGenericErrorLabelTemplate").Value.ToBase64();
                var genericResponse = new GenericResponse<ParcelDataResponse>(new ParcelDataResponse
                {
                    Zpl = errorTemplate ?? string.Empty
                });

                return Ok(genericResponse);
            }
        }

        [HttpPost(Name = nameof(NestParcelInActiveContainer))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ParcelDataResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<GenericResponse<NestParcelResponse>>> NestParcelInActiveContainer(NestParcelRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"{JsonSerializer.Serialize(request)}");

                var nestResponse = await autoScanProcessor.ProcessNestPackageRequest(request);
                
                logger.Log(LogLevel.Information, $"NestPackage Total Time: {nestResponse.Timer.TotalWatch}");
                logger.Log(LogLevel.Information, $"{JsonSerializer.Serialize(nestResponse.Response)}");
                var genericResponse = new GenericResponse<NestParcelResponse>(nestResponse.Response, !string.IsNullOrEmpty(nestResponse.Response.BinCode), nestResponse.Message);
                return Ok(genericResponse);
            }

            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"NestParcel Error for Site: {request.Site} Package ID: {request.Barcode} machine ID: {request.MachineId} Exception: {ex}");
                var genericResponse = new GenericResponse<NestParcelResponse>(new NestParcelResponse(), false, "NestParcel Error");
                return Ok(genericResponse);
            }
        }

        [HttpPost(Name = nameof(ConfirmParcelData))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ParcelConfirmDataResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<GenericResponse<ParcelConfirmDataResponse>>> ConfirmParcelData(ParcelConfirmDataRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"{JsonSerializer.Serialize(request)}");
                var response = await autoScanProcessor.ConfirmParcelData(request);

                var genericResponse = new GenericResponse<ParcelConfirmDataResponse>(response);

                logger.Log(LogLevel.Information, $"{JsonSerializer.Serialize(genericResponse)}");
                return Ok(genericResponse);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Information, $"ConfirmParcelData Error for Barcode: {request.Barcode} LogicalLane: {request.LogicalLane} Exception: {ex}");
                return Ok(new GenericResponse<ParcelConfirmDataResponse>(new ParcelConfirmDataResponse()));
            }
        }

        [HttpPost(Name = nameof(ValidateUser))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidateUserRequest), StatusCodes.Status200OK)]
        public ActionResult<GenericResponse<ValidateUserResponse>> ValidateUser(ValidateUserRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"{JsonSerializer.Serialize(request)}");
                var response = new ValidateUserResponse();

                var user = userManager.Users.FirstOrDefault(x => x.UserName == request.Username);

                if (user != null && !user.Deactivated)
                {
                    response.Confirmed = true;
                }

                var genericResponse = new GenericResponse<ValidateUserResponse>(response);
                logger.Log(LogLevel.Information, $"{JsonSerializer.Serialize(genericResponse)}");
                return Ok(genericResponse);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"ValidateUser Error for Username: {request.Username} Exception: {ex}");
                return Ok(new GenericResponse<ValidateUserResponse>(new ValidateUserResponse()));
            }
        }
    }
}