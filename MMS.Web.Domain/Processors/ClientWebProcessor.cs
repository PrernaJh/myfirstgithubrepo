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
    public class ClientWebProcessor : IClientWebProcessor
    {
        private readonly IClientRepository _clientRepository;

        public ClientWebProcessor(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<GetClientsResponse> GetClientsAsync(GetClientsRequest request)
        {
            var response = new GetClientsResponse();
            var clients = await _clientRepository.GetClientsAsync();
            var clientsResponse = new List<Client>();

            if (request.Name != IdentityDataConstants.Global)
            {
                clientsResponse.AddRange(clients.Where(x => x.Name == request.Name));
            }
            else
            {
                clientsResponse.AddRange(clients);
            }

            foreach (var client in clientsResponse)
            {
                response.GetClients.Add(new GetClient
                {
                    Name = client.Name,
                    Id = client.Id,
                    Description = client.Description
                });
            }

            return response;
        }

           
}
}
