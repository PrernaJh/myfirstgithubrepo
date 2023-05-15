using ParcelPrepGov.API.Client.Data;

namespace ParcelPrepGov.API.Client.Interfaces
{
    public interface IAccountService
    {
        AccountLogin PostAccountLogin(string username, string pwd);
        bool PostAccountLogout(AccountLogin acctToLogout);
        GetUserResponse GetUser(string username, AccountLogin loggedInAccount);
    }
}
