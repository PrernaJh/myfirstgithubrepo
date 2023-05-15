using System;

namespace MMS.API.Domain.Models
{
	public class ForceExceptionResponse
	{
		public string PackageId { get; set; }
		public bool Succeeded { get; set; }
		public DateTime ResponseDate { get; set; }
		public int LabelTypeId { get; set; }
		public string ErrorLabelMessage { get; set; }
	}
}
