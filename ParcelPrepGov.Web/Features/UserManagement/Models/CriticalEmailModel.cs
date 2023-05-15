using System.ComponentModel.DataAnnotations;

namespace ParcelPrepGov.Web.Features.UserManagement.Models
{
    public class CriticalEmailModel
    { 
        /// <summary>
        /// this computed property is used as a parameter for the delete function
        /// in devexpress datagrid
        /// </summary>
        public string EmailSiteNameId
        {
            get
            { 
                return @"{'Email': " + "'" + Email + "'," + "'SiteName': '" + SiteName + "'" + "}";
                
            }
        }

        [Required]
        public string SiteName { get; set; }

        [Required]
        [RegularExpression(@"^[\d\w._-]+@[\d\w._-]+\.[\w]+$", ErrorMessage = "Email is invalid")]
        public string Email { get; set; }
    }
}
