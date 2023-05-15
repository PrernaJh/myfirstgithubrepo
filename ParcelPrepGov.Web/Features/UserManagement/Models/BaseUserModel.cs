using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.UserManagement.Models
{
	public class BaseUserModel
	{
        public string Id { get; set; }
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }        
        [RegularExpression(@"^[\d\w\._\-]+@([\d\w\._\-]+\.)+[\w]+$", ErrorMessage = "Email is invalid")]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        public bool? SendRecallReleaseAlerts { get; set; }
        public bool? Deactivated { get; set; }
        public bool? ResetPassword { get; set; }
        public bool? SendEmail { get; set; }
        public int? ConsecutiveScansAllowed { get; set; }
        public DateTime? LastPasswordChangedDate { get; set; }
        public string TemporaryPassword { get; set; }
        [Required(ErrorMessage = "Client is required")]
        public string Client { get; set; }
        [Required(ErrorMessage = "SubClient is required")]
        public string SubClient { get; set; }
        [Required(ErrorMessage = "Site is required")]
        public string Site { get; set; }
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }
    }
}
