using System.Collections.Generic;

namespace ParcelPrepGov.Web.Features.Account.Models
{
    public class ProfileModel
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ConsecutiveScansAllowed { get; set; }
        public string Email { get; set; }
        public string Client { get; set; }
        public string SubClient { get; set; }
        public string Site { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Claims { get; set; }
    }
}
