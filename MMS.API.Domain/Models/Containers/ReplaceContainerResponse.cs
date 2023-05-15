using System.Collections.Generic;

namespace MMS.API.Domain.Models.Containers
{
    public class ReplaceContainerResponse
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }

        public List<string> PackageIdsUpdated { get; set; } = new List<string>();
    }
}
