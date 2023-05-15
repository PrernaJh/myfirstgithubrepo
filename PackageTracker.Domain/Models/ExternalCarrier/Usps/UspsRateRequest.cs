namespace PackageTracker.Domain.Models.ExternalCarrier.Usps
{
	public class UspsRateRequest
	{
		public string PackageId { get; set; }
		public string Service { get; set; }
		public string ZipOrigination { get; set; }
		public string ZipDestination { get; set; }
		public string Pounds { get; set; }
		public string Ounces { get; set; }
		public string Container { get; set; }
		public string Width { get; set; }
		public string Length { get; set; }
		public string Height { get; set; }
		public string Girth { get; set; }
	}
}
