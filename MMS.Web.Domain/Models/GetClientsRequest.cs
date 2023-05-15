using System;
using System.Collections.Generic;
using System.Text;

namespace MMS.Web.Domain.Models
{
    public class GetClientsRequest : BaseRequest
    {
        public string Name { get; set; }
    }
}
