using System.Collections.Generic;

namespace ParcelPrepGov.API.Client.Data
{
    public class ScannedContainerResponse
    {
        public string ContainerId { get; set; }
        public string BinCode { get; set; }
        public bool IsSecondaryCarrier { get; set; }
        public string HumanReadableBarcode { get; set; }
        public int LabelTypeId { get; set; }
        public IEnumerable<LabelFieldValue> LabelFieldValues { get; set; } = new List<LabelFieldValue>();
    }
}
