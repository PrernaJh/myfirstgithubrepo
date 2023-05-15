namespace PackageTracker.Domain.Models.ExternalCarrier
{
	public class UspsLabelRequest
	{
		public string ReturnName { get; set; }
		public string ReturnAddressLine1 { get; set; }
		public string ReturnAddressLine2 { get; set; }
		public string ReturnCity { get; set; }
		public string ReturnState { get; set; }
		public string ReturnZip { get; set; }
		public string RecipientName { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Zip { get; set; }
		public string IsPoBox { get; set; }
		public string WeightInOunces { get; set; }
		public string ServiceTypeRequestCode { get; set; }
		public string ContainerRequestCode { get; set; }
		public string ServiceNameRequestCode { get; set; }
		public string Width { get; set; }
		public string Length { get; set; }
		public string Height { get; set; }
		public string Machinable { get; set; }
		public string MailerId { get; set; }
		public string ImageType { get; set; }

	}

	public class ImageParameters
	{
		public string ImageParameter { get; set; }
		public string XCoordinate { get; set; }
		public string YCoordinate { get; set; }
	}
}
