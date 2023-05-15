using PackageTracker.Data.Models;

namespace MMS.API.Domain.Models.OperationalContainers
{
    public class GetOperationalContainerResponse
    {
        public OperationalContainer OperationalContainer { get; set; }
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
    }
}
