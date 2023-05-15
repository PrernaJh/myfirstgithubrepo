using System.Collections.Generic;

namespace ParcelPrepGov.Web.Features.UserManagement.Models
{
	public class ClientViewModel
	{
		public string ClientName { get; set; }
		public bool AllClients { get; set; }
		public List<string> AssignableClients { get; set; }
	}
}
