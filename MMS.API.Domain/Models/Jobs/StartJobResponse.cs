namespace MMS.API.Domain.Models
{
	public class StartJobResponse
	{
		public string JobBarcode { get; set; }
		public string Product { get; set; }
		public string MarkUp { get; set; }
		public string ManifestDate { get; set; }
		public string BillOfLading { get; set; }
		public string Reference { get; set; }
		public string SerialNumber { get; set; }
		public string ReceivedAt { get; set; }
		public string PackageDimensions { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
