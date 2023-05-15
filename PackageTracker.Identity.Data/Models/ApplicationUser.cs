using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace PackageTracker.Identity.Data.Models
{
	public class ApplicationUser : IdentityUser
	{
		public ApplicationUser()
		{
			LastPasswordChangedDate = DateTime.UtcNow;
		}

		[MaxLength(50)]
		public string Site { get; set; }
		[MaxLength(50)]
		public string Client { get; set; }
		[MaxLength(50)]
		public string SubClient { get; set; }
		public int ConsecutiveScansAllowed { get; set; }
		[MaxLength(200)]
		public string FirstName { get; set; }
		[MaxLength(200)]
		public string LastName { get; set; }
		public bool Deactivated { get; set; }
		public bool ResetPassword { get; set; }
		public DateTime LastPasswordChangedDate { get; set; }
		[MaxLength(200)]
		public string TemporaryPassword { get; set; }
		[MaxLength(200)]
		public string SecurityCode { get; set; }
		public DateTime SecurityCodeExpirationDate { get; set; }
		public DateTime TemporaryPasswordExpirationDate { get; set; }
		public bool SendRecallReleaseAlerts { get; set; }
	}
}
