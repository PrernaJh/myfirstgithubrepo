using Microsoft.AspNetCore.Authentication;
using PackageTracker.Identity.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PackageTracker.Identity.Service.Interfaces
{
    public interface IExternalSignInManager
    {
        Task<bool> CanExternalUserRegisterAsync();

        Task<string> CreateUserFromExternalLoginAsync();

        AuthenticationProperties GetAuthenticationProperties(string authenticationScheme, string redirectUrl);

        Task<ApplicationUser> FindUserByIdAsync(string userId);

        string GetExternalUserEmail();

        Task<HashSet<string>> GetExternalUserExistingRolesAsync();

        string GetExternalUserFirstName();

        string GetExternalUserLastName();

        string GetExternalUserSite();

        Task<bool> LinkExternalLoginWithUserAsync();

        Task<bool> SignInAsync();

        Task UpdateUserRolesIfNeededAsync();
    }
}
