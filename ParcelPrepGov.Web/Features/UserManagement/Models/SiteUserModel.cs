using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.UserManagement.Models
{
	public class SiteUserModel
	{
		public SiteUserModel()
		{
			Roles = new List<string>();
			ResetPassword = true;
		}

		public string Id { get; set; }
		[Required(ErrorMessage = "UserName is required")]
		public string UserName { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		[RegularExpression(@"^[\d\w\._\-]+@([\d\w\._\-]+\.)+[\w]+$", ErrorMessage = "Email is invalid")]
		public string Email { get; set; }
		public bool Deactivated { get; set; }
		public bool ResetPassword { get; set; }
		public DateTime LastPasswordChangedDate { get; set; }
		public string TemporaryPassword { get; set; }
		public string Site { get; set; }
		public List<string> Roles { get; set; }
	}
}
