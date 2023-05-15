using ParcelPrepGov.Reports.Models.SprocModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParcelPrepGov.Reports.Interfaces
{
	public interface IReportsRepository
	{
		Task<IEnumerable<AdvancedDailyWarningMaster>> GetAdvancedDailyWarningDetailMasters(string subClientNames, string beginDate, string endDate);
		Task<IEnumerable<AdvancedDailyWarningDetail>> GetAdvancedDailyWarningDetailDetails(string subClientNames, string beginDate, string endDate, string id = null);
		Task<IEnumerable<DailyRevenueFile>> GetDailyRevenueFile(string subClientNames, string manifestDate);
		Task<IEnumerable<DailyPackageSummary>> GetDailyPackageSummary(string siteName, string manifestDate);
		Task<IEnumerable<ClientDailyPackageSummary>> GetClientDailyPackageSummary(string subClientNames, string manifestDate, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<Undelivered>> GetPostalPerformanceNoStc(string subClientNames, string beginDate, string endDate, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<UspsGtr5Detail>> GetUspsGtr5Details(string subClientNames, string beginDate, string endDate, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<UspsCarrierDetailMaster>> GetUspsCarrierDetailMaster(string subClientName, string beginDate, string endDate, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<UspsCarrierDetailDetail>> GetUspsCarrierDetailDetails(string subClientName, string beginDate, string endDate, string id = null, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<UspsDropPointStatusMaster>> GetUspsDropPointStatusMaster(string subClientNames, string beginDate, string endDate, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<UspsDropPointStatusDetail>> GetUspsDropPointStatusDetails(string subClientNames, string beginDate, string endDate, string id = null, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<UspsDPSByContainerMaster>> GetUspsDropPointStatusByContainerMaster(string siteName, string beginDate, string endDate, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<UspsDPSByContainerDetail>> GetUspsDropPointStatusByContainerDetails(string siteName, string beginDate, string endDate, string id = null, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<UspsUndeliverableMaster>> GetUspsUndeliverableMaster(string subClientNames, string beginDate, string endDate);
		Task<IEnumerable<UspsUndeliverableDetail>> GetUspsUndeliverableDetails(string subClientNames, string beginDate, string endDate, string id = null);
		Task<IEnumerable<UspsDailyPieceDetail>> GetUspsDailyPieceDetail(string subClientNames, string manifestDate, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<UpsDailyPieceDetail>> GetUpsDailyPieceDetail(string subClientNames, string manifestDate, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<DailyPieceDetail>> GetDailyPieceDetail(string subClientNames, string manifestDate, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<FedExDailyPieceDetail>> GetFedExDailyPieceDetail(string subClientNames, string manifestDate, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<PostalPerformanceSummary>> GetPostalPerformanceSummary(string subClientNames, string beginDate, string endDate, IDictionary<string, string> filterBy = null);
		Task<(IEnumerable<PostalPerformance3Digit> ThreeDigitDetails, IEnumerable<PostalPerformance5Digit> FiveDigitDetails)> GetPostalPerformanceSummary3DigitAnd5Digit(string id);
		Task<IEnumerable<PostalPerformanceGtr6>> GetPostalPerformanceGtr6(string siteName, string beginDate, string endDate);
		Task<IEnumerable<WeeklyInvoiceFile>> GetWeeklyInvoiceFile(string subClientNames, string beginDate, string endDate);
		Task<IEnumerable<UspsLocationDeliverySummary>> GetUspsLocationDeliverySummary(string subClientNames, string beginDate, string endDate);
		Task<IEnumerable<UspsProductDeliverySummary>> GetUspsProductDeliverySummary(string subClientNames, string beginDate, string endDate);
		Task<IEnumerable<UspsVisnDeliverySummary>> GetUspsVisnDeliverySummary(string subClientNames, string beginDate, string endDate);
		Task<IEnumerable<UspsLocationTrackingSummary>> GetUspsLocationTrackingSummary(string subClientNames, string beginDate, string endDate);
		Task<IEnumerable<UspsVisnTrackingSummary>> GetUspsVisnTrackingSummary(string subClientNames, string beginDate, string endDate);
		Task<IEnumerable<RecallReleaseSummary>> GetRecallReleaseSummary(string subClientNames, string beginDate, string endDate);
		Task<IEnumerable<CarrierDetail>> GetCarrierDetail(string siteName, string beginDate, string endDate, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<AsnReconciliationDetailMaster>> GetAsnReconciliationDetailMaster(string subClientName, string beginDate, string endDate);
		Task<IEnumerable<BasicContainerPackageNesting>> GetBasicContainerPackageNesting(string siteName, string manifestDate, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<DailyContainerMaster>> GetDailyContainerMaster(string siteName, string manifestDate, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<DailyContainerDetail>> GetDailyContainerDetails(string siteName, string manifestDate, string id = null, IDictionary<string, string> filterBy = null);
		Task<IEnumerable<USPSMonthlyDeliveryPerformanceSummary>> GetUSPSMonthlyDeliveryPerformanceSummary(string subClientName, string startDate, string endDate);
	}
}
