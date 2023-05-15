namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
	public class ManageServiceRulesViewModel
	{
		public ManageServiceRulesViewModel()
		{
			UploadServiceRulesViewModel = new UploadServiceRulesViewModel();
			SearchServiceRulesViewModel = new SearchServiceRulesViewModel();
		}

		public UploadServiceRulesViewModel UploadServiceRulesViewModel { get; set; }
		public SearchServiceRulesViewModel SearchServiceRulesViewModel { get; set; }
	}

}
