using System;
using System.Collections.Generic;
using System.Text;
using ParcelPrepGov.Reports.Attributes;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class USPSMonthlyDeliveryPerformanceSummary
    {
        public string SubClientName { get; set; }
        public string ManifestMonth { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Product { get; set; }
        public int TOTAL_PIECES { get; set; }        
        public decimal AVG_POSTAL_DAYS { get; set; }             
        public decimal AVG_CAL_DAYS { get; set; }
        public int TOTAL_PCS_NO_STC { get; set; }
        public int TOTAL_PCS_NO_SCAN { get; set; }

        [DisplayName("<= Day 3")]        
        public int LessThanOrEqualToDay3 { get; set; } // <= 3
        public int Day4 { get; set; }
        public int Day5 { get; set; }
        public int Day6 { get; set; }
        public int Day7 { get; set; }
        public int Day8 { get; set; }
        public int Day9 { get; set; }
        public int Day10 { get; set; }
        public int Day11 { get; set; }
        public int Day12 { get; set; }
        public int Day13 { get; set; }
        public int Day14 { get; set; }

        [DisplayName(">= Day 15")]        
        public int GreaterOrEqualTo15 { get; set; }        
    }
}
