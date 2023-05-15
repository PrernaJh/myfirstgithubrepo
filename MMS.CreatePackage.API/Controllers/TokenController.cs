using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace MailManifestSystem.CreatePackageAPI
{
	[Authorize]
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class TokenController : ControllerBase
	{
		private readonly ILogger<TokenController> logger;

		public TokenController(ILogger<TokenController> logger)
		{
			this.logger = logger;
		}

		[HttpPost]
		[ProducesResponseType(typeof(ValidateTokenResponse), 200)]
		public async Task<IActionResult> ValidateToken(ValidateTokenRequest request)
		{
			try
			{
				var result = new ValidateTokenResponse();
				var timer = Stopwatch.StartNew();
				logger.LogInformation($"ValidateToken: {JsonSerializer.Serialize(request)}");

				result.IsSuccessful = true;
				result.Message = $"Token is valid";
				var serializedResponse = JsonSerializer.Serialize(result);

				timer.Stop();
				logger.LogInformation($"ValidateToken: {serializedResponse} Total Time: {timer.ElapsedMilliseconds}");

				return Ok(serializedResponse);
			}
			catch (Exception ex)
			{
				logger.LogError("ValidateToken Exception: " + ex);
				return Ok(new ValidateTokenResponse());
			}
		}
	}
}
