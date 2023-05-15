using ParcelPrepGov.Reports.Attributes;
using System;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class UspsUndeliverableDetail
    {
        public string ID { get; set; }
        [DisplayFormatAttribute("DATE")]
        public DateTime? MANIFEST_DATE { get; set; }
        public string MANIFEST_DATE_STRING
        {
            get
            {
                return MANIFEST_DATE == null ? string.Empty : MANIFEST_DATE.Value.ToString("MM/dd/yyyy");
            }
        }
        public string CUST_LOCATION { get; set; }
        public string VISN { get; set; }
        public string MEDICAL_CENTER_NO { get; set; }
        public string MEDICAL_CENTER_NAME { get; set; }
        public string PACKAGE_ID { get; set; }
        public static string HOST { get; set; }
        public string PACKAGE_ID_HYPERLINK
        {
            get
            {
                return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatPackageHyperLink(HOST, PACKAGE_ID);
            }
        }
        public string DESTINATION_ZIP { get; set; }
        public string TRACKING_NUMBER { get; set; }
        public string TRACKING_NUMBER_HYPERLINK
        {
            get
            {
                return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatTrackingHyperLink(PACKAGE_CARRIER, TRACKING_NUMBER);
            }
        }
        public string CARRIER { get; set; }
        [ExcelIgnore]
        public string PACKAGE_CARRIER { get; set; }
 
        public string UNDELIVERABLE_DESC { get; set; }
        [DisplayFormatAttribute("DATE_TIME")]
        public DateTime? UNDELIVERABLE_DATE_TIME { get; set; }
        public string UNDELIVERABLE_DATE_TIME_STRING
        {
            get
            {
                return UNDELIVERABLE_DATE_TIME == null ? string.Empty : UNDELIVERABLE_DATE_TIME.Value.ToString("MM/dd/yyyy hh:mm tt");
            }
        }
        public string LAST_KNOWN_EVENT_DESC { get; set; }
        [DisplayFormatAttribute("DATE_TIME")]
        public DateTime? LAST_KNOWN_EVENT_DATE_TIME { get; set; }
        public string LAST_KNOWN_EVENT_DATE_TIME_STRING
        {
            get
            {
                return LAST_KNOWN_EVENT_DATE_TIME == null ? string.Empty : LAST_KNOWN_EVENT_DATE_TIME.Value.ToString("MM/dd/yyyy hh:mm tt");
            }
        }
    }
}
