using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Data.Models
{
    public class CarrierApiError
    {
        public string Severity { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
}
