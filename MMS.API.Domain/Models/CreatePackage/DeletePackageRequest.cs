namespace MMS.API.Domain.Models.CreatePackage
{
	public class DeletePackageRequest : BaseRequest
	{
		public string SubClient { get; set; }
		public string PackageId { get; set; }
	}
}
