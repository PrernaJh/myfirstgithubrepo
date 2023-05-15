namespace MMS.API.Domain.Models.AutoScan
{
	public class ParcelDataResponse
	{
		// return package.bincode
		public string LogicalName { get; set; }

		//return package.barcode in verify property
		public string Verify { get; set; }

		public string Zpl { get; set; }

        public bool PrintLabel { get; set; }
    }
}