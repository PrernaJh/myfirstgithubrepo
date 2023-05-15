namespace MMS.API.Domain.Models.CreatePackage
{
	public class CreatePackageRequest : BaseRequest
	{
		public string PackageId { get; set; }        
        public string SubClient { get; set; }
		public string ClientFacility { get; set; }		
		public string FromName { get; set; }
		public string FromFirm { get; set; }
		public string FromAddressLine1 { get; set; }
		public string FromAddressLine2 { get; set; }
		public string FromCity { get; set; }
		public string FromState { get; set; }
		public string FromZip5 { get; set; }
		public string FromZip4 { get; set; }
		public string FromPhone { get; set; }
		public string ToName { get; set; }
		public string ToFirm { get; set; }
		public string ToAddressLine1 { get; set; }
		public string ToAddressLine2 { get; set; }
		public string ToCity { get; set; }
		public string ToState { get; set; }
		public string ToZip5 { get; set; }
		public string ToZip4 { get; set; }
		public string ToPhone { get; set; }
		public bool IsPoBox { get; set; }
		public bool IsOrmd { get; set; }
		public string WeightInOunces { get; set; }
		public string BusinessRuleType { get; set; }
		public string TrackingRuleType { get; set; }
		public string BinRuleType { get; set; }
		public string DimensionRuleType { get; set; }
		public string Carrier { get; set; }
		public string ShippingMethod { get; set; }
		public string TrackingNumber { get; set; }
		public string BinCode { get; set; }
		public string Width { get; set; }
		public string Length { get; set; }
		public string Height { get; set; }
		public string Girth { get; set; }
		public string ShipDate { get; set; }
		public string CustomerReferenceNumber { get; set; }
		public string CustomerReferenceNumber2 { get; set; }
		public string CustomerReferenceNumber3 { get; set; }
		public string CustomerReferenceNumber4 { get; set; }
        public bool GenerateShippingLabel { get; set; }
    }
}
