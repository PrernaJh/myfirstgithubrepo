using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace PackageTracker.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PingController : ControllerBase
	{
		private readonly ILogger<PingController> logger;

		public PingController(ILogger<PingController> logger)
		{
			this.logger = logger;
		}

		[HttpGet]
		[ProducesResponseType(200)]
		public IActionResult Ping()
		{
			try
			{
				return Ok();
			}
			catch (Exception ex)
			{
				logger.Log(LogLevel.Error, $"Failed to ping server. Exception: {ex}");
				return BadRequest($"Failed to ping server");
			}
		}
	}
}