using System;

namespace PackageTracker.Data.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string EventType { get; set; }
        public string EventStatus { get; set; }
        public string Description { get; set; }
        public string TrackingNumber { get; set; }
        public string Username { get; set; }
        public string MachineId { get; set; }        
        public DateTime EventDate { get; set; }
        public DateTime LocalEventDate { get; set; }
    }
}
