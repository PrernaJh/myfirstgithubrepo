using System;
using System.Collections.Generic;
using System.Text;

namespace MMS.Web.Domain.Models
{
    public class GetClientsResponse : BaseResponse
    {
        public List<GetClient> GetClients { get; set; } = new List<GetClient>();
    }
}
