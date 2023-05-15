using System.Collections.Generic;

namespace MMS.API.Domain.ZplUtilities
{
	public class ZPLConfiguration
	{
		public const string ZPLSection = "ZPLSettings";

		public string AutoScanUspsLabelTemplate { get; set; }
		public string AutoScanUpsGroundLabelTemplate { get; set; }
		public string AutoScanUpsAirLabelTemplate { get; set; }
		public string AutoScanReturnLabelTemplate { get; set; }
		public string AutoScanErrorLabelTemplate { get; set; }
		public string ThreeLineLabelTemplate { get; set; }
        public List<CreatePackageTemplate> CreatePackageTemplates { get; set; }
    }
}