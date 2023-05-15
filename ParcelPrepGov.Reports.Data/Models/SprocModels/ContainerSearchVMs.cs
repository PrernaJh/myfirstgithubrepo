using System;
using System.Collections.Generic;
using System.Text;
using PackageTracker.Domain.Utilities;
using PackageTracker.Identity.Data.Constants;
using ParcelPrepGov.Reports.Attributes;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class ContainerSearchResultViewModel
    {
        // always return one container..
        // container info        
        public static string HOST { get; set; }
        [ExcelIgnore]
        public string ID { get; set; } // UI UNIQUE

        public string CONTAINER_ID { get; set; }
        public string CONTAINER_ID_HYPERLINK
        {
            get
            {
                return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatContainerHyperLink(HOST, CONTAINER_ID);
            }
        }

        [DisplayFormatAttribute("DATE")]
        public DateTime? MANIFEST_DATE { get; set; }
        public string MANIFEST_DATE_STRING 
        {
            get
            {             
                return MANIFEST_DATE.GetMonthDateYearOnly();
            }            
        }
        public string STATUS { get; set; }
        public string CONTAINER_TYPE { get; set; }
        public string SHIPPING_CARRIER { get; set; }
        public string SHIPPING_METHOD { get; set; } 
        public string TRACKING_NUMBER { get; set; }
        public string TRACKING_NUMBER_HYPERLINK 
        {
            get
            {
                return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatTrackingHyperLink(SHIPPING_CARRIER, TRACKING_NUMBER);
            }
        }

        // location details
        public string BIN_CODE { get; set; }
        public string FSC_SITE { get; set; }
        public string DROP_SHIP_SITE_KEY { get; set; }
        public string ENTRY_UNIT_NAME { get; set; }
        public string ENTRY_UNIT_CSZ { get; set; }
        public string ENTRY_UNIT_TYPE { get; set; }
        public string ZONE { get; set; }
        public bool IS_SECONDARY_CARRIER { get; set; }
        public bool IS_OUTSIDE_48_STATES { get; set; }
        public bool IS_RURAL { get; set; }
        public bool IS_SATURDAY { get; set; }

        // Container Diagnostics
        public string CONTAINER_WEIGHT { get; set; }
        public int PIECES_IN_CONTAINER { get; set; }

        [DisplayFormatAttribute("DATE_TIME")]
        public DateTime? LAST_KNOWN_DATE { get; set; }
        public string LAST_KNOWN_DATE_STRING
        {
            get
            {
                return LAST_KNOWN_DATE.HasValue ? Convert.ToDateTime(LAST_KNOWN_DATE).ToString("MM/dd/yyyy hh:mm tt") : string.Empty;
            }
        }
        public string LAST_KNOWN_DESCRIPTION { get; set; }
        public string LAST_KNOWN_LOCATION { get; set; }
        public string LAST_KNOWN_ZIP { get; set; }
        
        [ExcelIgnore]
        public IEnumerable<ContainerEventsViewModel> EVENTS { get; set; } = new List<ContainerEventsViewModel>();

        [ExcelIgnore]
        public IEnumerable<ContainerSearchPacakgeViewModel> PACKAGES { get; set; } = new List<ContainerSearchPacakgeViewModel>();
    }
    public class ContainerEventsViewModel
    {
        public static string HOST { get; set; }

        [DisplayFormatAttribute("DATE_TIME_WS")]
        public DateTime LOCAL_EVENT_DATE { get; set; }

        [ExcelIgnore]
        public string SHIPPING_CARRIER { get; set; }
        public string TRACKING_NUMBER { get; set; }
        public string TRACKING_NUMBER_HYPERLINK
        {
            get
            {
                return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatTrackingHyperLink(SHIPPING_CARRIER, TRACKING_NUMBER);
            }
        }        
        public string EVENT_TYPE { get; set; }
        public string EVENT_STATUS { get; set; }
        public string USER_NAME { get; set; }

        [ExcelIgnore(new string[] { PPGRole.SubClientWebAdministrator, PPGRole.SubClientWebUser, 
            PPGRole.ClientWebAdministrator, PPGRole.ClientWebUser, PPGRole.CustomerService })]
        public string DISPLAY_NAME { get; set; }
        public string MACHINE_ID { get; set; }
    }
    public class ContainerSearchPacakgeViewModel 
    {
        [ExcelIgnore]
        public int ID { get; set; }
        public static string HOST { get; set; }
        public string PACKAGE_ID { get; set; }        
        public string PACKAGE_ID_HYPERLINK
        {
            get
            {
                return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatPackageHyperLink(HOST, PACKAGE_ID);
            }
        }

        [ExcelIgnore]
        public string SUBCLIENT_NAME { get; set; }

        [ExcelIgnore]
        public string PACKAGE_STATUS { get; set; }

        [ExcelIgnore]
        public DateTime? RECALL_DATE { get; set; }

        [ExcelIgnore]
        public string RECALL_STATUS { get; set; }


        public string TRACKING_NUMBER { get; set; }        
        public string TRACKING_NUMBER_HYPERLINK
        {
            get
            {
                return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatTrackingHyperLink(CARRIER, TRACKING_NUMBER);
            }
        }

        [DisplayName("PACKAGE_CARRIER")]
        public string CARRIER { get; set; }
        public string SHIPPING_METHOD { get; set; }

        [DisplayFormatAttribute("DATE_TIME")]
        public DateTime LAST_KNOWN_DATE { get; set; }
        public string LAST_KNOWN_DATE_STRING
        {
            get
            {
                return LAST_KNOWN_DATE == null ? string.Empty : Convert.ToDateTime(LAST_KNOWN_DATE).ToString("MM/dd/yyyy hh:mm tt");
            }
        }
        public string LAST_KNOWN_DESC { get; set; }
        public string LAST_KNOWN_LOCATION { get; set; }
        public string LAST_KNOWN_ZIP { get; set; }      
    }    
}
