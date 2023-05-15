namespace PackageTracker.Data.Models
{
	public class Rate : Entity
	{
		public Rate()
		{
		}

		public Rate(Rate other)
        {
			ActiveGroupId = other.ActiveGroupId;
			Carrier = other.Carrier;
			Service = other.Service;
			ContainerType = other.ContainerType;
			WeightNotOverOz = other.WeightNotOverOz;
			CostZone1 = other.CostZone1;
			CostZone2 = other.CostZone2;
			CostZone3 = other.CostZone3;
			CostZone4 = other.CostZone4;
			CostZone5 = other.CostZone5;
			CostZone6 = other.CostZone6;
			CostZone7 = other.CostZone7;
			CostZone8 = other.CostZone8;
			CostZone9 = other.CostZone9;
			CostZoneDdu = other.CostZoneDdu;
			CostZoneScf = other.CostZoneScf;
			CostZoneNdc = other.CostZoneNdc;
			ChargeZone1 = other.ChargeZone1;
			ChargeZone2 = other.ChargeZone2;
			ChargeZone3 = other.ChargeZone3;
			ChargeZone4 = other.ChargeZone4;
			ChargeZone5 = other.ChargeZone5;
			ChargeZone6 = other.ChargeZone6;
			ChargeZone7 = other.ChargeZone7;
			ChargeZone8 = other.ChargeZone8;
			ChargeZone9 = other.ChargeZone9;
			ChargeZoneDdu = other.ChargeZoneDdu;
			ChargeZoneScf = other.ChargeZoneScf;
			ChargeZoneNdc = other.ChargeZoneNdc;
			IsRural = other.IsRural;
			IsOutside48States = other.IsOutside48States;
		}
		public string ActiveGroupId { get; set; }
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
		public decimal CostZoneDduOut48 { get; set; }
		public decimal CostZoneScfOut48 { get; set; }
		public decimal CostZoneNdcOut48 { get; set; }
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
		public decimal ChargeZoneDduOut48 { get; set; }
		public decimal ChargeZoneScfOut48 { get; set; }
		public decimal ChargeZoneNdcOut48 { get; set; }
		public bool IsRural { get; set; }
		public bool IsOutside48States { get; set; }
	}
}
