using System.Collections.Generic;

namespace PackageTracker.Data.Models
{
    public class ClientFacility : Entity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ClientName { get; set; }
        public List<ClientFacilityRule> ClientFacilityRules { get; set; }
        public string TimeZone { get; set; }
    }
}