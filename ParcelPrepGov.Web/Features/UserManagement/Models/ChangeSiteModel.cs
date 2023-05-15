using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.UserManagement.Models
{
	public class ChangeSiteModel
	{
		[Required()]
		public string UserId { get; set; }
		[Required()]
		public string Site { get; set; }
		public string Username { get; set; }
	}
}
