using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ParcelPrepGov.Web.Models
{
	public class SectionSelectorModel
	{
		public IEnumerable<SelectListItem> Clients { get; set; }
		public IEnumerable<SelectListItem> SubClients { get; set; }
		public IEnumerable<SelectListItem> Sites { get; set; }
		public string SelectedSite { get; set; }
		public string SelectedClient { get; set; }
		public string SelectedSubClient { get; set; }
	}
}
