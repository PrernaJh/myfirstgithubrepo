using PackageTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Domain.Models
{
    public class FedExShippingDataResponse
    {        
        public bool Successful { get; set; } = true;
        public string Message { get; set; }
        public string Barcode { get; set; }
        public string Base64Label { get; set; }        
    }
}
