using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Communications.Models
{
    public class EmailAttachment
    {
        public string MimeType { get; set; } // e.g. "text/plain", etc.
        public string FileName { get; set; }
        public byte[] FileContents { get; set; }
    }
}
