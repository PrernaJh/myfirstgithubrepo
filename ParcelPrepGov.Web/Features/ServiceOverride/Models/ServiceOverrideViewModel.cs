using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;

namespace ParcelPrepGov.Web.Features.ServiceOverride.Models
{
	public class ServiceOverrideViewModel
	{
		public ServiceOverrideViewModel()
		{
			ShippingCarriers = new List<string>();
			ShippingMethods = new List<string>();
		}

		public List<string> ShippingCarriers { get; set; }
		public List<string> ShippingMethods { get; set; }
		public List<ActiveGroup> ServiceOverrides { get; set; }

		public ServiceOverridePost ServiceOverridePost { get; set; }
        public DateTime SiteLocalTime { get; set; }
    }


}
