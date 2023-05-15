namespace MMS.API.Domain.Models.ProcessScanAndAuto
{
    public class ProcessPackage
    {
        public string Username { get; set; }
        public string MachineId { get; set; }
        public string JobId { get; set; }
        public string SiteNameRequest { get; set; }
        public string PackageIdRequest { get; set; }
        public string ErrorLabelMessage { get; set; }
        public bool ProcessingCompleted { get; set; }
        public bool ShouldUpdate { get; set; }
        public bool IsJobAssigned { get; set; }
        public bool IsBinVerified { get; set; }
        public bool IsCreatedPackage { get; set; }
        public decimal Weight { get; set; }
    }
}
