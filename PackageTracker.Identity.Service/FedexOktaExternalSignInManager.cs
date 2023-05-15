using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using PackageTracker.Data.Constants;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Identity.Data;
using PackageTracker.Identity.Data.Models;
using PackageTracker.Identity.Service.Interfaces;
using ParcelPrepGov.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PackageTracker.Identity.Service
{
    public class FedexOktaExternalSignInManager : IExternalSignInManager
    {
        private readonly Dictionary<string, string> _cachedExternalUserClaims = new Dictionary<string, string>();
        private readonly ExternalLoginInfo _externalLoginInfo;
        private readonly HashSet<string> _cachedExternalUserRoles = new HashSet<string>();
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserLookupProcessor _userLookupProcessor;

        private readonly ISiteProcessor _siteProcessor;

        public FedexOktaExternalSignInManager(
            ISiteProcessor siteProcessor,
            RoleManager<IdentityRole> roleManager, 
            SignInManager<ApplicationUser> signInManager, 
            UserManager<ApplicationUser> userManager
        )
        {
            _roleManager = roleManager;
            _signInManager = signInManager;
            _siteProcessor = siteProcessor;
            _userManager = userManager;

            var getExternalInfoTask = _signInManager.GetExternalLoginInfoAsync();
            _externalLoginInfo = getExternalInfoTask.Result;
        }

        public async Task<bool> CanExternalUserRegisterAsync()
        {
            var allAvailableSiteNames = await _siteProcessor.GetAllSiteNamesAsync();
            var externalUserRoles = await GetExternalUserExistingRolesAsync();
            var externalUserSite = GetExternalUserSite();
            var rolesExists = externalUserRoles.Count > 0;
            var siteExists = allAvailableSiteNames.SiteNames.Exists(siteName => siteName.Equals(externalUserSite, StringComparison.OrdinalIgnoreCase));

            return !string.IsNullOrWhiteSpace(GetExternalUserEmail()) && rolesExists && siteExists;
        }

        public async Task<string> CreateUserFromExternalLoginAsync()
        {
            var newUserFromExternalLogin = await GetNewUserFromExternalInfoAsync();

            var createResult = await _userManager.CreateAsync(newUserFromExternalLogin);

            if (!createResult.Succeeded)
            {
                throw new ExternalLoginException($"Unable to create user ({newUserFromExternalLogin.Email}) from external login info");
            }

            var addUserToRoleResults = new List<IdentityResult>();
            var rolesToAddToUser = await GetExternalUserExistingRolesAsync();

            foreach (var role in rolesToAddToUser)
            {
                addUserToRoleResults.Add(await _userManager.AddToRoleAsync(newUserFromExternalLogin, role));
            }

            if (addUserToRoleResults.Count == 0 || addUserToRoleResults.Any(result => !result.Succeeded))
            {
                throw new ExternalLoginException($"Unable to add user ({newUserFromExternalLogin.Email}) to roles ({string.Join(", ", rolesToAddToUser)})");
            }

            if (!await TryAddExternalLoginAsync(newUserFromExternalLogin))
            {
                throw new ExternalLoginException($"Unable to establish external login link for user {newUserFromExternalLogin.Email}");
            }

            // Add user to reports UserLookups.
            await _userLookupProcessor.UpsertUser(newUserFromExternalLogin);

            return newUserFromExternalLogin.Id;
        }

        public async Task<ApplicationUser> FindUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public AuthenticationProperties GetAuthenticationProperties(string authenticationScheme, string redirectUrl)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(authenticationScheme, redirectUrl);
        }

        public string GetExternalUserEmail()
        {
            return GetExternalUserClaim("email", claim => claim.Type == "preferred_username");
        }

        public async Task<HashSet<string>> GetExternalUserExistingRolesAsync()
        {
            if (_cachedExternalUserRoles.Count > 0)
            {
                return _cachedExternalUserRoles;
            }

            var externalUserRoleClaims = _externalLoginInfo.Principal.Claims.Where(claim => claim.Type == ClaimTypes.Role && claim.Value != "Everyone");

            foreach (var roleClaim in externalUserRoleClaims)
            {
                var role = roleClaim.Value;

                if (await _roleManager.RoleExistsAsync(role))
                {
                    _cachedExternalUserRoles.Add(role);
                }
            }

            return _cachedExternalUserRoles;
        }

        public string GetExternalUserFirstName()
        {
            return GetExternalUserClaim("firstName", claim => claim.Type == "given_name");
        }

        public string GetExternalUserLastName()
        {
            return GetExternalUserClaim("lastName", claim => claim.Type == "family_name");
        }

        public string GetExternalUserSite()
        {
            return GetExternalUserClaim("site", claim => claim.Type == ClaimTypes.Locality);
        }

        public async Task<bool> LinkExternalLoginWithUserAsync()
        {
            var userEmail = GetExternalUserEmail();

            var possibleExistingUser = await _userManager.FindByEmailAsync(userEmail);

            if (possibleExistingUser == null)
            {
                return false;
            }

            if (await TryAddExternalLoginAsync(possibleExistingUser) && await TryClearExternalUserPasswordAsync(possibleExistingUser))
            {
                return await TryExternalLoginSignInAsync(_externalLoginInfo);
            }

            return false;
        }

        public async Task<bool> SignInAsync()
        {
            return await TryExternalLoginSignInAsync(_externalLoginInfo);
        }

        public async Task UpdateUserRolesIfNeededAsync()
        {
            var userOfInterest = await _userManager.FindByEmailAsync(GetExternalUserEmail());
            var usersRolesInIdentity = (await _userManager.GetRolesAsync(userOfInterest)).ToHashSet();
            var usersRolesInOkta = await GetExternalUserExistingRolesAsync();

            if (usersRolesInOkta.Count != 0 && !usersRolesInOkta.SetEquals(usersRolesInIdentity))
            {
                var rolesToRemove = usersRolesInIdentity.Except(usersRolesInOkta);

                await _userManager.RemoveFromRolesAsync(userOfInterest, rolesToRemove);

                var rolesToAdd = usersRolesInOkta.Except(usersRolesInIdentity);

                await _userManager.AddToRolesAsync(userOfInterest, rolesToAdd);
            }
        }

        private string GetExternalUserClaim(string claimKey, Func<Claim, bool> userClaimPredicate)
        {
            if (!_cachedExternalUserClaims.TryGetValue(claimKey, out string claimValue))
            {
                var claim = _externalLoginInfo.Principal.Claims.FirstOrDefault(userClaimPredicate);

                if (claim != null)
                {
                    claimValue = claim.Value;
                    _cachedExternalUserClaims.Add(claimKey, claimValue);
                }
            }

            return claimValue;
        }

        private async Task<ApplicationUser> GetNewUserFromExternalInfoAsync()
        {
            var externalUserEmail = GetExternalUserEmail();

            return new ApplicationUser
            {
                Email = externalUserEmail,
                UserName = await SafelyGetNewUsernameAsync(externalUserEmail),
                Site = GetExternalUserSite(),
                FirstName = GetExternalUserFirstName(),
                LastName = GetExternalUserLastName(),
                Client = ClientSubClientConstants.AllClients,
                SubClient = ClientSubClientConstants.AllSubClients,
            };
        }

        private async Task<string> SafelyGetNewUsernameAsync(string email)
        {
            ApplicationUser possibleExistingUser;
            var newUsernameTemplate = email.Substring(0, email.IndexOf('@'));
            var numToAppendIfDuplicate = 0;
            var possibleUsername = newUsernameTemplate;

            do
            {
                if (numToAppendIfDuplicate > 0)
                {
                    possibleUsername = $"{newUsernameTemplate}{numToAppendIfDuplicate}";
                }

                possibleExistingUser = await _userManager.FindByNameAsync(possibleUsername);
                numToAppendIfDuplicate++;
            }
            while (possibleExistingUser != null);

            return possibleUsername;
        }

        private async Task<bool> TryAddExternalLoginAsync(ApplicationUser user)
        {
            var createExternalLoginLinkResult = await _userManager.AddLoginAsync(user, _externalLoginInfo);

            return createExternalLoginLinkResult.Succeeded;
        }

        private async Task<bool> TryClearExternalUserPasswordAsync(ApplicationUser user)
        {
            user.PasswordHash = null;

            var clearPasswordResult = await _userManager.UpdateAsync(user);

            return clearPasswordResult.Succeeded;
        }

        private async Task<bool> TryExternalLoginSignInAsync(ExternalLoginInfo info)
        {
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            return signInResult.Succeeded;
        }
    }
}
