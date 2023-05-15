using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MMS.API.Domain.Interfaces;
using MMS.API.Domain.Models;
using PackageTracker.Identity.Data.Constants;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PackageTracker.API.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IJobScanProcessor jobProcessor;
        private readonly ILogger<JobController> logger;

        public JobController(IJobScanProcessor jobProcessor, ILogger<JobController> logger)
        {
            this.jobProcessor = jobProcessor;
            this.logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = IdentityDataConstants.GMAdminSuperQA)]
        [ProducesResponseType(typeof(AddJobResponse), 200)]
        public async Task<IActionResult> AddJob(AddJobRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"AddJob Request: {JsonSerializer.Serialize(request)}");
                var response = await jobProcessor.AddJobAsync(request);
                var serializedResponse = JsonSerializer.Serialize(response);
                logger.Log(LogLevel.Information, $"AddJob Response: {serializedResponse}");
                return Ok(serializedResponse);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to create new job. Exception: {ex}");
                return BadRequest($"Failed to create new job");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(StartJobResponse), 200)]
        public async Task<IActionResult> StartJob(StartJobRequest request)
        {
            try
            {
                logger.Log(LogLevel.Information, $"StartJob Request: {JsonSerializer.Serialize(request)}");
                var response = await jobProcessor.GetStartJob(request);

                var serializedResponse = JsonSerializer.Serialize(response);

                if (!response.IsSuccessful)
                {
                    logger.Log(LogLevel.Information, $"Job not found: {serializedResponse}");
                    return Ok(serializedResponse);
                }

                logger.Log(LogLevel.Information, $"StartJob Response: {serializedResponse}");
                return Ok(serializedResponse);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to start job. Exception: {ex}");
                return BadRequest($"Failed to start job");
            }
        }

        [HttpGet]
        [Authorize(Roles = IdentityDataConstants.GMAdminSuperQA)]
        [ProducesResponseType(typeof(GetJobOptionResponse), 200)]
        public async Task<IActionResult> GetJobOptions(string siteName)
        {
            try
            {
                logger.Log(LogLevel.Information, $"GetJobOptions Request: {JsonSerializer.Serialize(siteName)}");
                var response = await jobProcessor.GetJobOptionAsync(siteName);
                var serializedResponse = JsonSerializer.Serialize(response);
                logger.Log(LogLevel.Information, $"GetJobOptions Response: {serializedResponse}");
                return Ok(serializedResponse);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to get job rules. Exception: {ex}");
                return BadRequest($"Failed to get job rules");
            }
        }
    }
}