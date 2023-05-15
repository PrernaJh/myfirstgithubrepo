using System.Collections.Generic;

namespace MMS.Web.Domain.Models
{
    public class GetSubClientsResponse : BaseResponse
    {
        public List<GetSubClient> GetSubClients { get; set; } = new List<GetSubClient>();
    }
}
