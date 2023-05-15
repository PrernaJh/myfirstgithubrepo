using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using PackageTracker.Data.Extensions;
using PackageTracker.Domain.Utilities;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models.SprocModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ParcelPrepGov.Reports.Repositories
{
	public class ReportsRepository : IReportsRepository
	{
		private readonly ILogger<ReportsRepository> logger;

		private readonly IConfiguration configuration;
		private readonly IPpgReportsDbContextFactory factory;

		private readonly IDbConnection connection;
		private readonly int commandTimeout;

		private readonly IDistributedCache cache;
		private double cacheItemTTL = 0;

		private int LoadCommandTimeout()
		{
			if (!int.TryParse(configuration.GetSection("SqlCommandTimeout").Value, out var commandTimeout))
				commandTimeout = 300; // seconds

			logger.LogInformation($"Command Timeout");
			return commandTimeout;
		}

		double LoadCacheTTL()
		{
			double ttl = configuration.GetSection("RedisCache:CacheItemTimeToLive").Get<double>();
			if (ttl == 0.0 || StringHelper.DoesNotExist(configuration.GetSection("ConnectionStrings:Redis").Value))
			{
				logger.LogInformation("Cache Disabled.");
				ttl = 0.0;
			}
			else
			{
				logger.LogInformation($"Cache Enabled: TTL: {ttl} minutes.");
			}
			return ttl;
		}

		public ReportsRepository(ILogger<ReportsRepository> logger,
			IConfiguration configuration,
			IDbConnection connection,
			IDistributedCache cache,
			IPpgReportsDbContextFactory factory)
		{
			this.logger = logger;

			this.configuration = configuration;
			this.connection = connection;
			this.cache = cache;
			this.factory = factory;

			commandTimeout = LoadCommandTimeout();
			cacheItemTTL = LoadCacheTTL();
		}

		private async Task<IEnumerable<T>> GetResults<T>(string query, bool useCache = true)
		{
			IEnumerable<T> response = new List<T>();
			var timer = Stopwatch.StartNew();
			try
			{
				var cacheRecordKey = $"ParcelPrepGov.Reports.Repositories.GetResults: {query}";
				response = (cacheItemTTL == 0.0 || !useCache) ? null : await cache.GetRecordAsync<IEnumerable<T>>(cacheRecordKey);
				if (response == null || response.Count() == 0)
				{
					response = await connection.QueryAsync<T>(query, commandTimeout: commandTimeout);
					if (cacheItemTTL != 0.0 && response.Count() > 0)
						await cache.SetRecordAsync<IEnumerable<T>>(cacheRecordKey, response, cacheItemTTL);
					timer.Stop();
					logger.LogInformation($"Cache Miss[{response.Count()},{timer.ElapsedMilliseconds / 1000.0}]: {cacheRecordKey}");
				}
				else
				{
					timer.Stop();
					logger.LogInformation($"Cache Hit[{response.Count()},{timer.ElapsedMilliseconds / 1000.0}]: {cacheRecordKey}");
				}
			}
			catch (Exception e)
			{
				logger.Log(LogLevel.Error, e.Message);
			}
			return response;
		}

		static string CanonicalDate(string dateString)
		{
			if (DateTime.TryParse(dateString, out var date))
				dateString = date.ToString("yyyy-MM-dd");
			return dateString;
		}

		private async Task<IEnumerable<T>> GetResults<T>(string storedProcedureName, string locations, string beginDate, string endDate = null, bool useCache = true)
		{
			if (StringHelper.DoesNotExist(locations))
				return new List<T>();
			string query = $"EXEC {storedProcedureName} '{locations}', '{CanonicalDate(beginDate)}'";
			if (endDate != null)
				query += $", '{CanonicalDate(endDate)}'";
			return await GetResults<T>(query, useCache);
		}

		public async Task<IEnumerable<AdvancedDailyWarningMaster>> GetAdvancedDailyWarningDetailMasters(string subClientNames, string beginDate, string endDate)
		{
			return await GetResults<AdvancedDailyWarningMaster>("getRptAdvancedDailyReport_master", subClientNames, beginDate, endDate);
		}

		public async Task<IEnumerable<AdvancedDailyWarningDetail>> GetAdvancedDailyWarningDetailDetails(string subClientNames, string beginDate, string endDate, string id)
		{
			var response = await GetResults<AdvancedDailyWarningDetail>("getRptAdvancedDailyReport_detail", subClientNames, beginDate, endDate);
			if (id != null)
				response = response.Where(d => d.ID == id);
			return response;
		}

		public async Task<IEnumerable<DailyRevenueFile>> GetDailyRevenueFile(string subClientNames, string manifestDate)
		{
			return await GetResults<DailyRevenueFile>("getRptDailyRevenueFile", subClientNames, manifestDate);
		}

		public async Task<IEnumerable<DailyPackageSummary>> GetDailyPackageSummary(string siteName, string manifestDate)
		{
			return await GetResults<DailyPackageSummary>("getRptDailyPackageSummary", siteName, manifestDate, null, false);
		}

		public async Task<IEnumerable<ClientDailyPackageSummary>> GetClientDailyPackageSummary(string subClientNames, string manifestDate, IDictionary<string, string> filterBy)
		{
			var query = $"EXEC getRptClientDailyPackageSummary '{subClientNames}', '{CanonicalDate(manifestDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("PRODUCT", filterBy);
			}
			return await GetResults<ClientDailyPackageSummary>("getRptClientDailyPackageSummary", subClientNames, manifestDate, null, false);
		}


		public async Task<IEnumerable<Undelivered>> GetPostalPerformanceNoStc(string subClientNames, string beginDate, string endDate, IDictionary<string, string> filterBy)
		{
			var query = $"EXEC getRptUndeliveredReport '{subClientNames}', '{CanonicalDate(beginDate)}', '{CanonicalDate(endDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("PRODUCT", filterBy);
				query += AddFilterParameter("USPS_AREA", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
				query += AddFilterParameter("LAST_KNOWN_DESC", filterBy);
			}
			return await GetResults<Undelivered>(query);
		}

		public async Task<IEnumerable<UspsGtr5Detail>> GetUspsGtr5Details(string subClientNames, string beginDate, string endDate, IDictionary<string, string> filterBy)
		{
			var query = $"EXEC getRptUspsGtr5Detail '{subClientNames}', '{CanonicalDate(beginDate)}', '{CanonicalDate(endDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("PRODUCT", filterBy);
				query += AddFilterParameter("USPS_AREA", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
				query += AddFilterParameter("LAST_KNOWN_DESC", filterBy);
			}
			return await GetResults<UspsGtr5Detail>(query);
		}

		public async Task<IEnumerable<UspsCarrierDetailMaster>> GetUspsCarrierDetailMaster(string subClientName, string beginDate, string endDate, IDictionary<string, string> filterBy)
		{
			var query = $"EXEC getRptUspsCarrierDetail_master '{subClientName}', '{CanonicalDate(beginDate)}', '{CanonicalDate(endDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("PRODUCT", filterBy);
			}
			return await GetResults<UspsCarrierDetailMaster>(query);
		}

		/// <summary>
		/// add filter parameter to return a ienumerable collection of objects
		/// </summary>
		/// <param name="subClientName"></param>
		/// <param name="beginDate"></param>
		/// <param name="endDate"></param>
		/// <param name="id"></param>
		/// <param name="filterBy"></param>
		/// <returns></returns>
		public async Task<IEnumerable<UspsCarrierDetailDetail>> GetUspsCarrierDetailDetails(string subClientName, string beginDate, string endDate, string id, IDictionary<string, string> filterBy)
		{
			if (id != null)
			{
				var response = await GetResults<UspsCarrierDetailDetail>("getRptUspsCarrierDetail_detail", subClientName, beginDate, endDate);
				// query again
				response = response.Where(d => d.ID == id);
				return response;
			}
			else
			{
				var query = $"EXEC getRptUspsCarrierDetail_detail '{subClientName}', '{CanonicalDate(beginDate)}', '{CanonicalDate(endDate)}'";
				if (filterBy != null)
				{
					query += AddFilterParameter("PRODUCT", filterBy);
				}

				return await GetResults<UspsCarrierDetailDetail>(query);
			}
		}

		public async Task<IEnumerable<UspsDropPointStatusMaster>> GetUspsDropPointStatusMaster(string subClientNames, string beginDate, string endDate, IDictionary<string, string> filterBy)
		{
			var query = $"EXEC getRptUspsDropPointStatus_master '{subClientNames}', '{CanonicalDate(beginDate)}', '{CanonicalDate(endDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("PRODUCT", filterBy);
				query += AddFilterParameter("USPS_AREA", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
			}
			return await GetResults<UspsDropPointStatusMaster>(query);
		}

		public async Task<IEnumerable<UspsDropPointStatusDetail>> GetUspsDropPointStatusDetails(string subClientNames, string beginDate, string endDate, string id, IDictionary<string, string> filterBy)
		{
			var query = $"EXEC getRptUspsDropPointStatus_detail '{subClientNames}', '{CanonicalDate(beginDate)}', '{CanonicalDate(endDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("PRODUCT", filterBy);
				query += AddFilterParameter("USPS_AREA", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
			}
			var response = await GetResults<UspsDropPointStatusDetail>(query);
			if (id != null)
				response = response.Where(d => d.ID == id).ToList();
			return response;
		}

		public async Task<IEnumerable<UspsDPSByContainerMaster>> GetUspsDropPointStatusByContainerMaster(string siteName, string beginDate, string endDate, IDictionary<string, string> filterBy)
		{
			var query = $"EXEC getRptUspsDropPointStatusByContainer_master '{siteName}', '{CanonicalDate(beginDate)}', '{CanonicalDate(endDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("PRODUCT", filterBy);
				query += AddFilterParameter("USPS_AREA", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
			}
			return await GetResults<UspsDPSByContainerMaster>(query);
		}

		public async Task<IEnumerable<UspsDPSByContainerDetail>> GetUspsDropPointStatusByContainerDetails(string siteName, string beginDate, string endDate, string id, IDictionary<string, string> filterBy)
		{
			var query = $"EXEC getRptUspsDropPointStatusByContainer_detail '{siteName}', '{CanonicalDate(beginDate)}', '{CanonicalDate(endDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("PRODUCT", filterBy);
				query += AddFilterParameter("USPS_AREA", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
			}
			var response = await GetResults<UspsDPSByContainerDetail>(query);
			if (id != null)
			{
				response = response.Where(d => d.ID == id).ToList();
			}

			return response;
		}

		public async Task<IEnumerable<UspsUndeliverableMaster>> GetUspsUndeliverableMaster(string subClientNames, string beginDate, string endDate)
		{
			return await GetResults<UspsUndeliverableMaster>("getRptUspsUndeliverables_master", subClientNames, beginDate, endDate);
		}

		public async Task<IEnumerable<UspsUndeliverableDetail>> GetUspsUndeliverableDetails(string subClientNames, string beginDate, string endDate, string id)
		{
			var response = await GetResults<UspsUndeliverableDetail>("getRptUspsUndeliverables_detail", subClientNames, beginDate, endDate);
			if (id != null)
				response = response.Where(d => d.ID == id);
			return response;
		}

		private string AddFilterParameter(string columnName, IDictionary<string, string> filterBy)
		{
			return filterBy.TryGetValue(columnName, out var value) && value != null ?
				", '" + value + "'" : ", NULL";
		}


		public async Task<IEnumerable<DailyPieceDetail>> GetDailyPieceDetail(string subClientNames, string manifestDate, IDictionary<string, string> filterBy)
		{
			var query = $"EXEC getRptDailyPieceDetail '{subClientNames}', '{CanonicalDate(manifestDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("PRODUCT", filterBy);
				query += AddFilterParameter("USPS_AREA", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
				query += AddFilterParameter("LAST_KNOWN_DESC", filterBy);
				query += AddFilterParameter("PACKAGE_CARRIER", filterBy);
			}
			return await GetResults<DailyPieceDetail>(query);
		}
	
		public async Task<IEnumerable<PostalPerformanceSummary>> GetPostalPerformanceSummary(string subClientNames, string beginDate, string endDate, IDictionary<string, string> filterBy = null)
		{
			var query = $"EXEC getRptPostalPerformanceSummary_master '{subClientNames}', '{CanonicalDate(beginDate)}', '{CanonicalDate(endDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("PRODUCT", filterBy);
				query += AddFilterParameter("USPS_AREA", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_TYPE", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
			}
			return await GetResults<PostalPerformanceSummary>(query);
		}

		public async Task<(IEnumerable<PostalPerformance3Digit> ThreeDigitDetails, IEnumerable<PostalPerformance5Digit> FiveDigitDetails)> GetPostalPerformanceSummary3DigitAnd5Digit(string id)
		{
			var fiveDigitDetails = new List<PostalPerformance5Digit>();
			var threeDigitDetails = await GetResults<PostalPerformance3Digit>($"EXEC getRptPostalPerformanceSummary_3d '{id}'");

			foreach (var threeDayDetail in threeDigitDetails)
			{
				fiveDigitDetails.AddRange(
					await GetResults<PostalPerformance5Digit>($"EXEC getRptPostalPerformanceSummary_5d '{threeDayDetail.ID3}'"));
			}
			return (threeDigitDetails, fiveDigitDetails);
		}

		public async Task<IEnumerable<PostalPerformanceGtr6>> GetPostalPerformanceGtr6(string siteName, string beginDate, string endDate)
		{
			return await GetResults<PostalPerformanceGtr6>("getRptPostalPerformanceSummary_gtr6", siteName, beginDate, endDate);
		}

		public async Task<IEnumerable<WeeklyInvoiceFile>> GetWeeklyInvoiceFile(string subClientNames, string beginDate, string endDate)
		{
			return await GetResults<WeeklyInvoiceFile>("getRptWeeklyInvoiceFile", subClientNames, beginDate, endDate);
		}

		public async Task<IEnumerable<UspsLocationDeliverySummary>> GetUspsLocationDeliverySummary(string subClientNames, string beginDate, string endDate)
		{
			return await GetResults<UspsLocationDeliverySummary>("getRptUspsLocationDeliverySummary", subClientNames, beginDate, endDate);
		}

		public async Task<IEnumerable<UspsProductDeliverySummary>> GetUspsProductDeliverySummary(string subClientNames, string beginDate, string endDate)
		{
			return await GetResults<UspsProductDeliverySummary>("getRptUspsProductDeliverySummary", subClientNames, beginDate, endDate);
		}

		public async Task<IEnumerable<UspsVisnDeliverySummary>> GetUspsVisnDeliverySummary(string subClientNames, string beginDate, string endDate)
		{
			return await GetResults<UspsVisnDeliverySummary>("getRptUspsVisnDeliverySummary", subClientNames, beginDate, endDate);
		}

		public async Task<IEnumerable<UspsLocationTrackingSummary>> GetUspsLocationTrackingSummary(string subClientNames, string beginDate, string endDate)
		{
			return await GetResults<UspsLocationTrackingSummary>("getRptUspsLocationTrackingSummary", subClientNames, beginDate, endDate);
		}

		public async Task<IEnumerable<UspsVisnTrackingSummary>> GetUspsVisnTrackingSummary(string subClientNames, string beginDate, string endDate)
		{
			return await GetResults<UspsVisnTrackingSummary>("getRptUspsVisnTrackingSummary", subClientNames, beginDate, endDate);
		}

		public async Task<IEnumerable<RecallReleaseSummary>> GetRecallReleaseSummary(string subClientNames, string beginDate, string endDate)
		{
			return await GetResults<RecallReleaseSummary>("getRptRecallReleaseSummary_master", subClientNames, beginDate, endDate);
		}

		public async Task<IEnumerable<CarrierDetail>> GetCarrierDetail(string siteName, string beginDate, string endDate, IDictionary<string, string> filterBy)
		{
			var query = $"EXEC getRptCarrierDetail '{siteName}', '{CanonicalDate(beginDate)}', '{CanonicalDate(endDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("ENTRY_UNIT_TYPE", filterBy);
				query += AddFilterParameter("CONTAINER_TYPE", filterBy);
				query += AddFilterParameter("CARRIER", filterBy);
				query += AddFilterParameter("CONTAINER_ID", filterBy);
				query += AddFilterParameter("CONTAINER_TRACKING_NUMBER", filterBy);
			}
			return await GetResults<CarrierDetail>(query);
		}

		public async Task<IEnumerable<AsnReconciliationDetailMaster>> GetAsnReconciliationDetailMaster(string subClientName, string beginDate, string endDate)
		{
			var query = $"EXEC getRptASNReconcilationDetail_master '{subClientName}', '{CanonicalDate(beginDate)}', '{CanonicalDate(endDate)}'";
			return await GetResults<AsnReconciliationDetailMaster>(query);
		}

		public async Task<IEnumerable<BasicContainerPackageNesting>> GetBasicContainerPackageNesting(string siteName, string manifestDate, IDictionary<string, string> filterBy = null)
		{
			var query = $"EXEC getRptBasicContainerPackageNesting '{siteName}', '{CanonicalDate(manifestDate)}'";
			if (filterBy != null)
            {
				query += AddFilterParameter("CUSTOMER", filterBy);
				query += AddFilterParameter("CONT_CARRIER", filterBy);
				query += AddFilterParameter("CONT_METHOD", filterBy);
				query += AddFilterParameter("CONT_TYPE", filterBy);
				query += AddFilterParameter("PKG_CARRIER", filterBy);
				query += AddFilterParameter("PKG_SHIPPINGMETHOD", filterBy);
				query += AddFilterParameter("SINGLE_BAG_SORT", filterBy);
			}

			return await GetResults<BasicContainerPackageNesting>(query);
		}

		public async Task<IEnumerable<DailyContainerMaster>> GetDailyContainerMaster(string siteName, string manifestDate, IDictionary<string, string> filterBy = null)
		{
			var query = $"EXEC getRptDailyContainer_master '{siteName}', '{CanonicalDate(manifestDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("CONT_TYPE", filterBy);
				query += AddFilterParameter("CARRIER", filterBy);
				query += AddFilterParameter("DROP_SHIP_SITE_KEY", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
			}
			return await GetResults<DailyContainerMaster>(query, false);
		}

		public async Task<IEnumerable<DailyContainerDetail>> GetDailyContainerDetails(string siteName, string manifestDate, string id = null, IDictionary<string, string> filterBy = null)
		{
			var query = $"EXEC getRptDailyContainer_detail '{siteName}', '{CanonicalDate(manifestDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("CONT_TYPE", filterBy);
				query += AddFilterParameter("CARRIER", filterBy);
				query += AddFilterParameter("DROP_SHIP_SITE_KEY", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
			}
			var response = await GetResults<DailyContainerDetail>(query, false);
			if (id != null)
				response = response.Where(d => d.ID == id).ToList();
			return response;
		}

        public async Task<IEnumerable<USPSMonthlyDeliveryPerformanceSummary>> GetUSPSMonthlyDeliveryPerformanceSummary(string subClientNames, string startDate, string endDate)
        {
			var query = $"EXEC getRptUSPSMonthlyDeliveryPerformanceSummary '{subClientNames}', '{CanonicalDate(startDate)}', '{CanonicalDate(endDate)}'";
			//if (filterBy != null)
			//{
			//	query += AddFilterParameter("PRODUCT", filterBy);
			//	query += AddFilterParameter("USPS_AREA", filterBy);
			//	query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
			//	query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
			//}
			var response = await GetResults<USPSMonthlyDeliveryPerformanceSummary>(query, false);
			
			return response;
		}

        #region Daily Piece Detail Not Condensed
        public async Task<IEnumerable<UpsDailyPieceDetail>> GetUpsDailyPieceDetail(string subClientNames, string manifestDate, IDictionary<string, string> filterBy)
		{
			var query = $"EXEC getRptUpsDailyPieceDetail '{subClientNames}', '{CanonicalDate(manifestDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("PRODUCT", filterBy);
				query += AddFilterParameter("USPS_AREA", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
				query += AddFilterParameter("LAST_KNOWN_DESC", filterBy);
				query += AddFilterParameter("PACKAGE_CARRIER", filterBy);

			}
			return await GetResults<UpsDailyPieceDetail>(query);
		}
		public async Task<IEnumerable<FedExDailyPieceDetail>> GetFedExDailyPieceDetail(string subClientNames, string manifestDate, IDictionary<string, string> filterBy)
		{
			var query = $"EXEC getRptFedExDailyPieceDetail '{subClientNames}', '{CanonicalDate(manifestDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("PRODUCT", filterBy);
				query += AddFilterParameter("USPS_AREA", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
				query += AddFilterParameter("LAST_KNOWN_DESC", filterBy);
			}
			return await GetResults<FedExDailyPieceDetail>(query);
		}
		public async Task<IEnumerable<UspsDailyPieceDetail>> GetUspsDailyPieceDetail(string subClientNames, string manifestDate, IDictionary<string, string> filterBy)
		{
			var query = $"EXEC getRptUspsDailyPieceDetail '{subClientNames}', '{CanonicalDate(manifestDate)}'";
			if (filterBy != null)
			{
				query += AddFilterParameter("PRODUCT", filterBy);
				query += AddFilterParameter("USPS_AREA", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_NAME", filterBy);
				query += AddFilterParameter("ENTRY_UNIT_CSZ", filterBy);
				query += AddFilterParameter("LAST_KNOWN_DESC", filterBy);
			}
			return await GetResults<UspsDailyPieceDetail>(query);
		}
        #endregion
    }
}
 