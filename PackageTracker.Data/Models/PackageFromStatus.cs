using System;
using System.Collections.Generic;

namespace PackageTracker.Data.Models
{
    public class PackageFromStatus
    {
        public string PackageId { get; set; }
        public string PackageStatus { get; set; }
        public string PackageStatusDescription { get; set; }
        public string RecallStatus { get; set; }
        public string RecordCreateDate { get; set; }
        public string RecallDate { get; set; }
        public string ReleaseDate { get; set; }
        private string _localProcessedDate;
        public string LocalProcessedDate
        {
            get
            {
                if (_localProcessedDate == "01/01/0001 00:00:00")
                {
                    return string.Empty;
                }
                return _localProcessedDate;
            }
            set { _localProcessedDate = value; }
        }
        public string SiteName { get; set; }
        public string ClientName { get; set; }
        public string SubClientName { get; set; }
        public string ContainerId { get; set; }
        public string BinCode { get; set; }
        public string ShippingCarrier { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

    }
}