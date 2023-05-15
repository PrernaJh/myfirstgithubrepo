using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Data.Models
{
    public class FedExCredentials
    {
        public string ApiKey { get; set; }
        public string ApiPassword { get; set; }
        public string AccountNumber { get; set; }
        public string MeterNumber { get; set; }
    }
}
