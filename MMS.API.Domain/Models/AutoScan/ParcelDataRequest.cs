using System.ComponentModel.DataAnnotations;

namespace MMS.API.Domain.Models.AutoScan
{
	public class ParcelDataRequest
	{
		[Required(ErrorMessage = "Barcode is Required")]
		public string Barcode { get; set; }

		[Required(ErrorMessage = "JobId is Required")]
		public string JobId { get; set; }

		[Required(ErrorMessage = "Username is Required")]
		public string Username { get; set; }

		[Required(ErrorMessage = "MachineId is Required")]
		public string MachineId { get; set; }

		[Required(ErrorMessage = "Site is Required")]
		public string Site { get; set; }

		[Required(ErrorMessage = "Weight is Required")]
		public decimal Weight { get; set; }
	}
}