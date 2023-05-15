using System;
using System.Collections.Generic;
using System.Text;

namespace ParcelPrepGov.API.Client.Data
{
    public class ReplaceContainer
    {
        public string siteName { get; set; }
        public string username { get; set; }
        public string machineId { get; set; }
        public string oldContainerId { get; set; }
        public string newContainerId { get; set; }
        public ReplaceContainerResponse response { get; set; } = new ReplaceContainerResponse();
    }
}
