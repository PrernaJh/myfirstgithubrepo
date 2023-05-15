using System;
using System.Collections.Generic;
using System.Text;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class PackageSearchEvent
    {        
        public int Id { get; set; }
        public DateTime DatasetCreateDate { get; set; } 
        public DateTime DatasetModifiedDate { get; set; }        
        public string CosmosId { get; set; }    
        public DateTime CosmosCreateDate { get; set; }        
        public string SiteName { get; set; }
        public int PackageDatasetId { get; set; }      
        public string PackageId { get; set; }        
        public string TrackingNumber { get; set; }
        public int EventId { get; set; }
        public string EventType { get; set; }        
        public string EventStatus { get; set; }        
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime LocalEventDate { get; set; }       
        public string Username { get; set; }        
        public string MachineId { get; set; }
        public string DisplayName { get; set; }
    }
}
