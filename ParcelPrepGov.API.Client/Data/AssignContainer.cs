namespace ParcelPrepGov.API.Client.Data
{
    public class AssignContainer
    {
        public string siteName { get; set; }
        public string username { get; set; }
        public string machineId { get; set; }
        public string packageId { get; set; }
        public string newContainerId { get; set; }
        public AssignContainerResponse response { get; set; } = new AssignContainerResponse();
    }
}
