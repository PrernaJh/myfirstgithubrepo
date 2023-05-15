using System;
using ParcelPrepGov.Reports.Attributes;

namespace ParcelPrepGov.Web.Features.RecallRelease.Models
{
    public class RecalledPackageViewModel : IPackageViewModel
    {
        public string PackageId { get; set; }   

        [DisplayNameAttribute("TrackingNumber")]        
        public string Barcode { get; set; }
        public string PackageStatus { get; set; }
        public string RecallStatus { get; set; }                
        public string RecallDate { get; set; }
        public string LocalProcessedDate { get; set; }
        public string ContainerId { get; set; }
        public string BinCode { get; set; }
        public string ShippingCarrier { get; set; }
        public string ShippingMethod { get; set; }
        
        [ExcelIgnore]
        public string ClientName { get; set; }

        [DisplayNameAttribute("CUST Location")]        
        public string SubClientName { get; set; }
        public string RecipientName { get; set; }
        public string Address { get
            {
                return $"{AddressLine1} {AddressLine2} {AddressLine3}";
            }
        }

        [ExcelIgnore]
        public string AddressLine1 { get; set; }
        [ExcelIgnore]
        public string AddressLine2 { get; set; }
        [ExcelIgnore]
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }        
        public string JobBarcode { get; set; }
        public string MailCode { get; set; }
        public string ProcessedDate { get; set; }
        public string SiteName { get; set; } //incase we have to format with a dash
    }
}