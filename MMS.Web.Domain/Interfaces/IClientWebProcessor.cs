using MMS.Web.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MMS.Web.Domain.Interfaces
{
    public interface IClientWebProcessor
    {
        Task<GetClientsResponse> GetClientsAsync(GetClientsRequest request);
    }
}
