using System;
using System.Collections.Generic;
using System.Text;
using ParcelPrepGov.Reports.Attributes;

namespace ParcelPrepGov.Reports.Models.SprocModels
{
    public class AsnReconciliationDetailMaster
    {
        [DisplayFormatAttribute("Package ID")]
        public string PACKAGE_ID { get; set; }
        public static string HOST { get; set; }
        public string PACKAGE_ID_HYPERLINK
        {
            get
            {
                return PackageTracker.Domain.Utilities.HyperLinkFormatter.FormatPackageHyperLink(HOST, PACKAGE_ID);
            }
        }
        [DisplayFormatAttribute("Import Date")]
        public string IMPORT_DATE { get; set; }
        public string SUB_CLIENT_NAME { get; set; }
        public string PACKAGE_STATUS { get; set; }
    }
}
