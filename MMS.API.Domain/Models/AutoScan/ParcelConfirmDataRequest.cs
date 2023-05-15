using System.ComponentModel.DataAnnotations;

namespace MMS.API.Domain.Models.AutoScan
{
	public class ParcelConfirmDataRequest
	{
		// same package id - barcode
		[Required(ErrorMessage = "Barcode is Required")]
		public string Barcode { get; set; }

		// LogicalLane => bin code
		[Required(ErrorMessage = "LogicalLane is Required")]
		public string LogicalLane { get; set; }
	}
}