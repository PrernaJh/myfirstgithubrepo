using MMS.Web.Domain.Interfaces;
using MMS.Web.Domain.Models;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using PackageTracker.Identity.Data.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMS.Web.Domain.Processors
{
    public class SubClientWebProcessor : ISubClientWebProcessor
    {
        private readonly ISubClientRepository subClientRepository;

        public SubClientWebProcessor(ISubClientRepository subClientRepository)
        {
            this.subClientRepository = subClientRepository;
        }

        public async Task<GetSubClientsResponse> GetSubclientsAsync(GetSubClientsRequest request)
        {
            var response = new GetSubClientsResponse();
            var subClients = await subClientRepository.GetSubClientsAsync();
            var subClientsResponse = new List<SubClient>();

            if (request.SubClientName != IdentityDataConstants.Global)
            {
                subClientsResponse.AddRange(subClients.Where(x => x.Name == request.SubClientName));
            }
            else if (request.SiteName != IdentityDataConstants.Global)
            {
                subClientsResponse.AddRange(subClients.Where(x => x.SiteName == request.SiteName));
            }
            else if (request.ClientName != IdentityDataConstants.Global)
            {
                subClientsResponse.AddRange(subClients.Where(x => x.ClientName == request.ClientName));
            }
            else
            {
                subClientsResponse.AddRange(subClients);
            }

            foreach (var subClient in subClientsResponse)
            {
                response.GetSubClients.Add(new GetSubClient
                {
                    Id = subClient.Id,
                    Description = subClient.Description,
                    Name = subClient.Name,
                    ClientName = subClient.ClientName,
                    SiteName = subClient.SiteName
                });
            }

            return response;
        }
    }
}
