using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Constants;
using PackageTracker.Data.Interfaces;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models.ActiveGroupDetails;
using PackageTracker.Identity.Data.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace PackageTracker.API.Controllers
{
	[Authorize]
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class ActiveGroupController : ControllerBase
	{
		private readonly IActiveGroupProcessor activeGroupProcessor;
		private readonly ILogger<ActiveGroupController> logger;
		private readonly ISubClientRepository subClientRepository;
		private readonly IWebJobRunRepository webJobRunRepository;

		public ActiveGroupController(IActiveGroupProcessor activeGroupProcessor, ILogger<ActiveGroupController> logger, ISubClientRepository subClientRepository, IWebJobRunRepository webJobRunRepository)
		{
			this.activeGroupProcessor = activeGroupProcessor;
			this.logger = logger;
			this.subClientRepository = subClientRepository;
			this.webJobRunRepository = webJobRunRepository;
		}

		[Authorize(Roles = IdentityDataConstants.SystemAdministrator)]
		[HttpGet]
		[ProducesResponseType(200)]
		[ProducesResponseType(typeof(GetServiceRuleDetailsResponse), 200)]
		public async Task<IActionResult> GetServiceRuleDetails()
		{
			try
			{
				var response = await activeGroupProcessor.GetServiceRuleDetails();
				return Ok(JsonSerializer.Serialize(response));
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to get current service rule details. Exception: {ex}");
				return BadRequest($"Failed to get current service rule details");
			}
		}

		[Authorize(Roles = IdentityDataConstants.SystemAdministrator)]
		[HttpGet]
		[ProducesResponseType(200)]
		[ProducesResponseType(typeof(GetBinDetailsResponse), 200)]
		public async Task<IActionResult> GetBinDetails()
		{
			try
			{
				var response = await activeGroupProcessor.GetBinDetails();
				return Ok(JsonSerializer.Serialize(response));
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to get current bin details. Exception: {ex}");
				return BadRequest($"Failed to get current bin details");
			}
		}

		[Authorize(Roles = IdentityDataConstants.SystemAdministrator)]
		[HttpGet]
		[ProducesResponseType(200)]
		public async Task<IActionResult> GetAsnWebJobRunsBySubClient(string subClientName)
		{
			try
			{
				var jobTypes = new List<string> { WebJobConstants.AsnImportJobType };
				var subClient = await subClientRepository.GetSubClientByNameAsync(subClientName);
				var timer = Stopwatch.StartNew();
				var response = await webJobRunRepository.GetAsnImportWebJobHistoryAsync(subClient.SiteName, subClientName);
				timer.Stop();
				return Ok(JsonSerializer.Serialize(timer.ElapsedMilliseconds));
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to get ASN import web job runs. Exception: {ex}");
				return BadRequest($"Failed to get ASN import web job runs");
			}
		}
	}
}
