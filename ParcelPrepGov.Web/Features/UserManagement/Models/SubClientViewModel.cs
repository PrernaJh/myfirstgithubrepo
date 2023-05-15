using System.Collections.Generic;

namespace ParcelPrepGov.Web.Features.UserManagement.Models
{
	public class SubClientViewModel
	{
		public string ClientName { get; set; }
		public string SubClientName { get; set; }
		public bool AllSubClients { get; set; }
		public List<string> AssignableSubClients { get; set; }
	}
}
