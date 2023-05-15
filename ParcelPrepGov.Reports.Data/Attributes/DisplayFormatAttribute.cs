using System;

namespace ParcelPrepGov.Reports.Attributes
{
    /// <summary>
    /// Attribute that controls how to format our excel exports
    /// </summary>
    public class DisplayFormatAttribute : Attribute
    {
        public DisplayFormatAttribute(string formatType, int precision = 0, string [] references = null)
        {
            FormatType = formatType;
            Precision = precision;
            References = references ?? new string[] { };
        }
        public string FormatType { get; set; }
        public int Precision { get; set; }
        public string [] References { get; set; }
    }
}
