using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Domain.Models
{
    public class TrackingDataImportResponse
    {
        public List<object> BadInputDocuments { get; set; } = new List<object>();
        public long NumberOfDocumentsImported { get; set; }
        public double RequestUnitsConsumed { get; set; }
        public TimeSpan HttpRequestDuration { get; set; }
        public TimeSpan DbInsertTime { get; set; }
        public TimeSpan SerializeTime { get; set; }
        public bool IsSuccessful { get; set; }
        public bool IsCompleted { get; set; }
    }
}
