using System;
using System.Collections.Generic;
using System.Text;

namespace ParcelPrepGov.Reports.Attributes
{
    public class ExcelIgnoreAttribute : Attribute    
    {
        public string[] Roles { get; set; }

        /// <summary>
        /// An empty attribute that works as a marker for removing columns for an excel export
        /// </summary>
        public ExcelIgnoreAttribute(string[] roles = null)
        {
            Roles = roles; 
        }
    }
}
