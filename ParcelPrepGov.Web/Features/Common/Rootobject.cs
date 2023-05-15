using System.Collections.Generic;

namespace ParcelPrepGov.Web.Features.Common
{
    public class Rootobject
    {
        public Pathinfo[] pathInfo { get; set; }

        public List<List<PathinfoList>> pathInfoList { get; set; }

        public Destinationpathinfo[] destinationPathInfo { get; set; }
        public string chunkMetadata { get; set; }
    }
     

    public class Destinationpathinfo
    {
        public string key { get; set; }
        public string name { get; set; }
    }

}
