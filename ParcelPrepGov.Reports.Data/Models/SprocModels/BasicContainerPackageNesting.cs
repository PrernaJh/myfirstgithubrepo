using System;
using System.Collections.Generic;
using ParcelPrepGov.Reports.Attributes;
using System.Text;
using PackageTracker.Identity.Data.Constants;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class BasicContainerPackageNesting
    {
        public static string HOST { get; set; }

        public string SITE { get; set; }
        public string CUSTOMER { get; set; }
        public string BINCODE { get; set; }
		public string DESTINATION { get; set; }
		public string CONTAINER_ID { get; set; }
        public string CONT_BARCODE { get; set; }
		public string CONT_CARRIER { get; set; }
		public string CONT_METHOD { get; set; }
        public string CONT_TYPE { get; set; }
        public string PACKAGE_ID { get; set; }
        public string TRACKING_NUMBER { get; set; }
        public string PKG_CARRIER { get; set; }
        public string PKG_SHIPPINGMETHOD { get; set; }
		public string PKG_PROCESSED_DATE { get; set; }
		[ExcelIgnore(new string[] { PPGRole.SubClientWebAdministrator, PPGRole.SubClientWebUser, PPGRole.ClientWebAdministrator, PPGRole.ClientWebUser, PPGRole.CustomerService })]
		public string PKG_PROCESSED_BY { get; set; }

        [ExcelIgnore(new string[] { PPGRole.SubClientWebAdministrator, PPGRole.SubClientWebUser, PPGRole.ClientWebAdministrator, PPGRole.ClientWebUser, PPGRole.CustomerService })]
		public string OPENED_BY_NAME { get; set; }		
        public string CONT_OPENED_DATE { get; set; }		

		[ExcelIgnore(new string[] { PPGRole.SubClientWebAdministrator, PPGRole.SubClientWebUser, PPGRole.ClientWebAdministrator, PPGRole.ClientWebUser, PPGRole.CustomerService })]
		public string CLOSED_BY_NAME { get; set; }
        public string CONT_CLOSED_DATE { get; set; }
		public string EVENT_TYPE { get; set; }
		public string MACHINE_ID { get; set; }
        public string DESTINATION_ZIP { get; set; }
		public string SITE_KEY { get; set; }
		public string SINGLE_BAG_SORT { get; set; }

		public string PACKAGE_ID_HYPERLINK
		{
			get
			{
				return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatPackageHyperLink(HOST, PACKAGE_ID);
			}
		}
		public string TRACKING_NUMBER_HYPERLINK
		{
			get
			{
				return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatTrackingHyperLink(PKG_CARRIER, TRACKING_NUMBER);
			}
		}
	}
}
