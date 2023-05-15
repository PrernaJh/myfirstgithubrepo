namespace MMS.API.Domain.Models.OperationalContainers
{
    public class GetOperationalContainerRequest
    {
        public string SiteName { get; set; }
        public string BinCode { get; set; }
        public string ContainerId { get; set; }
    }
}
