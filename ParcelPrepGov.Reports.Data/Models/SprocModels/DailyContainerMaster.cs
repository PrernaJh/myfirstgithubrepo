using System;
using System.Collections.Generic;
using ParcelPrepGov.Reports.Attributes;
using System.Text;
using PackageTracker.Identity.Data.Constants;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class DailyContainerMaster
    {
		public static string HOST { get; set; }
		public string ID { get; set; }
		public string CONTAINER_STATUS { get; set; }

		[DisplayFormatAttribute("DATE_TIME_WS")]
		public DateTime? CONTAINER_OPEN_DATE { get; set; }
        [ExcelIgnore(new string[] { PPGRole.SubClientWebAdministrator, PPGRole.SubClientWebUser, PPGRole.ClientWebAdministrator, PPGRole.ClientWebUser, PPGRole.CustomerService })]
		public string OPENED_BY_NAME { get; set; }
		public string TRACKING_NUMBER { get; set; }
		public string CONTAINER_ID { get; set; }
		public string CONTAINER_ID_HYPERLINK
		{
			get
			{
				return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatContainerHyperLink(HOST, CONTAINER_ID);
			}
		}

		public string BIN_NUMBER { get; set; }
        public string CONT_TYPE { get; set; }
        public string CONT_WEIGHT { get; set; }

		[DisplayFormatAttribute("COUNT")]
		public string TOTAL_PACKAGES { get; set; }
		public string CARRIER { get; set; }
		public string DROP_SHIP_SITE_KEY { get; set; }
		public string ENTRY_UNIT_NAME { get; set; }
		public string ENTRY_UNIT_CSZ { get; set; }
		public string CONTAINER_CLOSED_DATE 
		{
			get { return Convert.ToDateTime(_container_closed_date) == DateTime.MinValue ? string.Empty : _container_closed_date; }
			set { _container_closed_date = value; }
		}
        [ExcelIgnore(new string[] { PPGRole.SubClientWebAdministrator, PPGRole.SubClientWebUser, PPGRole.ClientWebAdministrator, PPGRole.ClientWebUser, PPGRole.CustomerService })]
		public string CLOSED_BY_NAME { get; set; }
		private string _container_closed_date;
	}
}
