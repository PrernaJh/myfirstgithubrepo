using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PackageTracker.API.Controllers
{
	[Authorize]
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class SiteController : ControllerBase
	{
		private readonly ILogger<SiteController> logger;
		private readonly ISiteProcessor siteProcessor;

		public SiteController(ILogger<SiteController> logger, ISiteProcessor siteProcessor)
		{
			this.logger = logger;
			this.siteProcessor = siteProcessor;
		}

		[HttpGet]
		[ProducesResponseType(200)]
		[ProducesResponseType(typeof(GetAllSiteNamesResponse), 200)]
		public async Task<IActionResult> GetAllSiteNames()
		{
			try
			{
				var response = await siteProcessor.GetAllSiteNamesAsync();
				return Ok(JsonSerializer.Serialize(response));
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to get all site names. Exception: {ex}");
				return BadRequest($"Failed to get all site names");
			}
		}

#if DEBUG
		[HttpPost]
		public async Task<GenericResponse<string>> AddUserToCriticalEmailList(ModifyCritialEmailListRequest request)
		{
			return await siteProcessor.AddUserToCriticalEmailList(request);
		}

		[HttpPost]
		public async Task<GenericResponse<string>> RemoveUserFromCriticalEmailList(ModifyCritialEmailListRequest request)
		{
			return await siteProcessor.RemoveUserFromCriticalEmailList(request);
		}
#endif
	}
}
