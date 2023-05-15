using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Features.ServiceManagement.Models
{
    public class RateViewModel
    {
		public string Carrier { get; set; }
		public string Service { get; set; }
		public string ContainerType { get; set; }
		public decimal WeightNotOverOz { get; set; }
		public decimal CostZone1 { get; set; }
		public decimal CostZone2 { get; set; }
		public decimal CostZone3 { get; set; }
		public decimal CostZone4 { get; set; }
		public decimal CostZone5 { get; set; }
		public decimal CostZone6 { get; set; }
		public decimal CostZone7 { get; set; }
		public decimal CostZone8 { get; set; }
		public decimal CostZone9 { get; set; }
		public decimal CostZoneDdu { get; set; }
		public decimal CostZoneScf { get; set; }
		public decimal CostZoneNdc { get; set; }
		public decimal ChargeZone1 { get; set; }
		public decimal ChargeZone2 { get; set; }
		public decimal ChargeZone3 { get; set; }
		public decimal ChargeZone4 { get; set; }
		public decimal ChargeZone5 { get; set; }
		public decimal ChargeZone6 { get; set; }
		public decimal ChargeZone7 { get; set; }
		public decimal ChargeZone8 { get; set; }
		public decimal ChargeZone9 { get; set; }
		public decimal ChargeZoneDdu { get; set; }
		public decimal ChargeZoneScf { get; set; }
		public decimal ChargeZoneNdc { get; set; }
		public bool IsRural { get; set; }
		public bool IsOutside48States { get; set; }
	}
}
