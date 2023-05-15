using PackageTracker.Data.Models;
using System.Collections.Generic;

namespace PackageTracker.Domain.Models.FileManagement
{
	public class ImportBinsAndBinMapsRequest
	{
		public ImportBinsAndBinMapsRequest()
		{
			Bins = new List<Bin>();
			BinMaps = new List<BinMap>();
		}

		public List<Bin> Bins { get; set; }
		public List<BinMap> BinMaps { get; set; }
		public string Filename { get; set; }
		public string SiteName { get; set; }
		public string SubClientName { get; set; }
		public string UserName { get; set; }
		public string StartDate { get; set; }
	}
}
