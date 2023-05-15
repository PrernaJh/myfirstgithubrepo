using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Web.Features.FileManagement.Models;
using ParcelPrepGov.Web.Infrastructure;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.FileManagement
{
	[Authorize]
	public partial class FileManagementController : Controller
	{
		[Authorize(PPGClaim.WebPortal.FileManagement.EndOfDayFiles)]
		public IActionResult EndOfDayFiles()
		{
			return View();
		}

		[AjaxOnly]
		[Authorize(PPGClaim.WebPortal.FileManagement.EndOfDayFiles)]
		[HttpPost(Name = nameof(GenerateFiles))]
		public async Task<IActionResult> GenerateFiles()
		{
			var displayMessages = new List<string>();
			var cacheKey = GenerateCacheKey();
			cache.TryGetValue(GenerateCacheKey(), out var value);
			if (value != null)
			{
				DateTime.TryParse(value.ToString(), out var timeStamp);
			}
			else
			{
				var queueClient = queueFactory.GetClient();
				var siteName = User.GetSite();
				var site = await siteProcessor.GetSiteBySiteNameAsync(siteName);
				var siteLocalTime = TimeZoneUtility.GetLocalTime(site.TimeZone);
				var triggerEodJobsQueue = config.GetSection("TriggerEodJobsQueue").Value;
				var fileConfigurations = await fileConfigurationProcessor.GetAllEndOfDayFileConfigurationsAsync();
				if (StringHelper.Exists(triggerEodJobsQueue))
                {
					var queueMessage = $"{siteName}_{User.Identity.Name}_{siteLocalTime:yyyy-MM-dd}"; // this datetime string must be parseable by the QueueUtility
					var queue = queueClient.GetQueueReference(triggerEodJobsQueue);
					await queue.AddMessageAsync(new CloudQueueMessage(queueMessage));
					logger.Log(LogLevel.Information, $"Job message {queueMessage} queued on server. { triggerEodJobsQueue }");
					foreach (var fileConfiguration in fileConfigurations)
					{
						displayMessages.Add($"Job queued on server: { fileConfiguration.FileDescription } {siteLocalTime:yyyy-MM-dd HH:mm:ss}");
					}
				}
                else
                {
					foreach (var fileConfiguration in fileConfigurations)
					{
						var destinationQueue = config.GetSection(fileConfiguration.ConfigurationName).Value;
						var queueMessage = $"{siteName}_{User.Identity.Name}_{siteLocalTime:yyyy-MM-dd}"; // this datetime string must be parseable by the QueueUtility
						var queue = queueClient.GetQueueReference(destinationQueue);
						await queue.AddMessageAsync(new CloudQueueMessage(queueMessage));
						displayMessages.Add($"Job queued on server: { fileConfiguration.FileDescription } {siteLocalTime:yyyy-MM-dd HH:mm:ss}");
						logger.Log(LogLevel.Information, $"Job message {queueMessage} queued on server. { fileConfiguration.FileDescription } {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
					}
                }
				cache.Set(cacheKey, DateTime.Now.AddMinutes(5));
			}

			return Json(new
			{
				success = true,
				displayMessages
			});
		}

		[AjaxOnly]
		[HttpGet(Name = nameof(GetEODJobHistory))]
		public async Task<JsonResult> GetEODJobHistory()
		{
			var eodHistories = new List<EODJobHistoryModel>();
			int.TryParse(config.GetSection("NumberOfDaysToFilter").Value, out int days);            	         			
			var eodJobHistoryWebJobs = await webJobRunProcessor.GetEndOfDayWebJobRunsAsync(User.GetSite(), days);

			foreach (var webJob in eodJobHistoryWebJobs)
			{
				foreach (var fileDetail in webJob.FileDetails)
				{
					eodHistories.Add(
						new EODJobHistoryModel
						{
							CreateDate = webJob.CreateDate,
							FileName = fileDetail.FileName,
							IsSuccessful = webJob.IsSuccessful,
							JobName = webJob.JobName,
							ErrorMessage = webJob.Message,
							Username = webJob.Username,
						});
				}
			}

			return new JsonResult(eodHistories);
		}

		private string GenerateCacheKey()
		{
			var key = $"{User.Identity.Name}_{User.GetSite()}_{DateTime.Now}";
			return key;
		}
	}
}
