using System.Collections.Generic;

namespace ParcelPrepGov.Web.Features.UserManagement.Models
{
	public class AdminViewModel
	{
		public List<string> Clients { get; set; }
		public List<string> SubClients { get; set; }
		public List<string> Sites { get; set; }
		public List<string> Roles { get; set; }
	}
}
