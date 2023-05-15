using System.ComponentModel.DataAnnotations;

namespace MMS.API.Domain.Models.AutoScan
{
    public class NestParcelRequest
    {
		[Required(ErrorMessage = "Barcode is Required")]
		public string Barcode { get; set; }

		[Required(ErrorMessage = "Username is Required")]
		public string Username { get; set; }

		[Required(ErrorMessage = "MachineId is Required")]
		public string MachineId { get; set; }

		[Required(ErrorMessage = "Site is Required")]
		public string Site { get; set; }
	}
}
