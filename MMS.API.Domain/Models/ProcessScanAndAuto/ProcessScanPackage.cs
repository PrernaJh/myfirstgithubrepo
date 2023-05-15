using MMS.API.Domain.Utilities;

namespace MMS.API.Domain.Models.ProcessScanAndAuto
{
    public class ProcessScanPackage : ProcessPackage
    {
        public bool IsRepeatScan { get; set; }
        public bool IsInvalidStatus { get; set; }
        public bool IsServiced { get; set; }
        public bool IsShipped { get; set; }
        public bool IsReturned { get; set; }
        public bool IsLabeled { get; set; }
        public PackageTimer Timer { get; set; } = new PackageTimer();
    }
}