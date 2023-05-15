using System;
using System.Collections.Generic;
using System.Text;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class UspsUndeliverableMaster
    {
        public string ID { get; set; }
        public string CUST_LOCATION { get; set; }
        public string VISN { get; set; }
        public string MEDICAL_CENTER_NO { get; set; }
        public string MEDICAL_CENTER_NAME { get; set; }
        public string EVENT_DESC { get; set; }
        public int? TOTAL_PCS { get; set; }
    }
}
