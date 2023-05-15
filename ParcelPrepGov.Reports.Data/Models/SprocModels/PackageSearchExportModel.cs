using ParcelPrepGov.Reports.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class PackageSearchExportModel
    {
        [ExcelIgnore]
        public static string HOST { get; set; }
        public int ID { get; set; }
        public string PACKAGE_ID { get; set; }
        public string PACKAGE_ID_HYPERLINK
        {
            get
            {
                return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatPackageHyperLink(HOST, PACKAGE_ID);
            }
        }
        public int? PACKAGE_DATA_SET_ID { get; set; }
        private string _inquiryId;
        public string INQUIRY_ID
        {
            get
            {
                return _inquiryId;
            }
            set { _inquiryId = value; }
        }
        
        public string INQUIRY_ID_HYPERLINK { get; set; }

        [ExcelIgnore]
        public string INQUIRY_ID_HYPERLINK_UIONLY { get; set; }

        public string SERVICE_REQUEST_NUMBER { get; set; }
        public string TRACKING_NUMBER { get; set; }
        public string TRACKING_NUMBER_HYPERLINK
        {
            get
            {
                return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatTrackingHyperLink(SHIPPING_CARRIER, TRACKING_NUMBER);
            }
        }

        [ExcelIgnore]
        public string CARRIER { get; set; }
        [DisplayNameAttribute("CARRIER")]
        public string SHIPPING_CARRIER { get; set; }
        public string PACKAGE_STATUS { get; set; }
        public string RECALL_STATUS { get; set; }

        [ExcelIgnore]
        public string SiteName { get; set; }

        public string CUST_LOCATION { get; set; }
        public string PRODUCT { get; set; }
        public string DEST_ZIP { get; set; }
        [DisplayFormatAttribute("DATE")]
        public DateTime? DATE_SHIPPED { get; set; }
        public DateTime? DATE_RECALLED { get; set; }
        public DateTime? DATE_RELEASED { get; set; }
        public string ENTRY_UNIT_NAME { get; set; }
        public string ENTRY_UNIT_CSZ { get; set; }
        public string LAST_KNOWN_DESC { get; set; }
        [DisplayFormatAttribute("DATE_TIME")]
        public DateTime? LAST_KNOWN_DATE { get; set; }
        public string LAST_KNOWN_LOCATION { get; set; }
        public string LAST_KNOWN_ZIP { get; set; }
    }
}
