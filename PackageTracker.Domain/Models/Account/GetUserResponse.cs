using System;
using System.Collections.Generic;
using System.Text;

namespace PackageTracker.Domain.Models.Account
{
    public class GetUserResponse
    {
		public string Username { get; set; }
		public string Role { get; set; }
		public string Email { get; set; }
		public string SiteName { get; set; }
	}
}
