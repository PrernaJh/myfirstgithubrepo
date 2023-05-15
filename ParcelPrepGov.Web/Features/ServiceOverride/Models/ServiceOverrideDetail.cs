using System;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.ServiceOverride.Models
{
    public class ServiceOverrideDetail 
    {
        private string _id;
        public string Id
        {
            get
            {
                return  $"{_id}|{Name}";
            }
            set
            {
                _id = value;
            }
        }
        public string Name { get; set; }
        public string AddedBy { get; set; }
        [Display(Name="Enable Override:")]
        public bool IsEnabled { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string OldShippingCarrier { get; set; }
        public string OldShippingMethod { get; set; }
        public string NewShippingCarrier { get; set; }
        public string NewShippingMethod { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
