using System;
using System.Collections.Generic;
using System.Text;

namespace ParcelPrepGov.API.Client.Data
{
    public class ScannedContainer
    {
        public string username { get; set; }
        public string machineId { get; set; }
        public string siteName { get; set; }
        public string containerId { get; set; }
        public string weight { get; set; }
        public ScannedContainerResponse response { get; set; } = new ScannedContainerResponse();
    }
}