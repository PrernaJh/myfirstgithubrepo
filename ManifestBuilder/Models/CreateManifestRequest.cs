using ManifestBuilder.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManifestBuilder
{
    public class CreateManifestRequest
    {
        public IEnumerable<Package> Packages;

        public IEnumerable<ShippingContainer> Containers;

        public Dictionary<string, int> EFNStartSequenceByMID;

        public DateTime MailDate;

        public string MailProducerMid;
        
        public Site Site;
        public bool IsForPmodContainers;
    }
}
