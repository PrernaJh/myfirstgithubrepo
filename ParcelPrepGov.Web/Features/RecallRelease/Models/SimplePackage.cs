using System;

namespace ParcelPrepGov.Web.Features.RecallRelease.Models
{
    public class SimplePackage
    {
        public string PackageId { get; set; }
        public string MailCode { get; set; }
        public string JobBarcode { get; set; }
        public string PackageStatus { get; set; }
        public string RecallStatus { get; set; }
        public string RecallDate { get; set; }
        public string ProcessedDate { get; set; } 

        public string LocalProcessedDate { get; set; }
        public string SiteName { get; set; }
        public string SubClientName { get; set; } 
    }
}