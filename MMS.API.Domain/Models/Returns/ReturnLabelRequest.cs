namespace MMS.API.Domain.Models.Returns
{
    public class ReturnLabelRequest
    {
        public string SiteName { get; set; }
        public string PackageId { get; set; }
        public string ReturnReason { get; set; }
        public string ReturnDescription { get; set; }
    }
}