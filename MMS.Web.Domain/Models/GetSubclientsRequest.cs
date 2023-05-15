namespace MMS.Web.Domain.Models
{
    public class GetSubClientsRequest : BaseRequest
    {
        public string SiteName { get; set; }
        public string ClientName { get; set; }
        public string SubClientName { get; set; }
    }
}
