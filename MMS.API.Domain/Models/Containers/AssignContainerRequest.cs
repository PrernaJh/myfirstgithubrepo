namespace MMS.API.Domain.Models.Containers
{
    public class AssignContainerRequest
    {
        public string PackageId { get; set; }
        public string NewContainerId { get; set; }
        public string SiteName { get; set; }
        public string Username { get; set; }
        public string MachineId { get; set; }
    }
}
