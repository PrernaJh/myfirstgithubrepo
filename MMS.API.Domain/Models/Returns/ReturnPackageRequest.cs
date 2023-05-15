namespace MMS.API.Domain.Models.Returns
{
	public class ReturnPackageRequest : PackageRequest
	{
		public string ReturnReasonValue { get; set; }
		public string ReasonDescriptionValue { get; set; }
	}
}