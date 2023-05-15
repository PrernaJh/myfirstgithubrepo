using System;

namespace ParcelPrepGov.Reports.Attributes
{
    /// <summary>
    /// An attribute that overides the column header name in excel
    /// </summary>
    public class DisplayNameAttribute : Attribute
    {
        public DisplayNameAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
