using System.Net;

namespace ParcelPrepGov.API.Client.Data
{
    public class AccountLogin
    {
        public string username { get; set; }
        public Cookie cookie { get; set; }
        public LoginResponse response { get; set; } = new LoginResponse();
    }
}
