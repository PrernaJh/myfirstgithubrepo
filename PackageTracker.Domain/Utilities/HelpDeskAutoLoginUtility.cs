using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using PackageTracker.Identity.Data.Models;
using System;
using System.Threading.Tasks;

namespace PackageTracker.Domain.Utilities
{
    public sealed class HelpDeskAutoLoginUtility
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public HelpDeskAutoLoginUtility(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        public async Task<string> GenerateHelpDeskUrlAsync(ApplicationUser User, string secret)
        {
            var helpDeskAutoLoginUrl = string.Empty;
            var sharedKey = _config.GetSection("HelpDeskSettings").GetSection("shared-key").Value;

            if (secret.Equals(sharedKey))
            {                
                helpDeskAutoLoginUrl = await GenerateHelpDeskUrlAsync(User.UserName, User.Email, secret);
            }

            return helpDeskAutoLoginUrl;
        }

        /// <summary>
        ///	Base method to login a user from Web into HelpDesk with a UserHash
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<string> GenerateHelpDeskUrlAsync(string userName, string email, string secret)
        {
            // Will redirect to http://https://help.mailmanifestsystem.com/User/AutoLogin?username=xxx&email=yyy&userHash=HASH
            DateTime date = DateTime.Now;
            string helpDeskAutoLoginUrl = null;
            var day = date.ToString("dd");
            var month = date.ToString("MM");
            var sharedKey = _config.GetSection("HelpDeskSettings").GetSection("shared-key").Value;

            if (secret.Equals(sharedKey))
            {
                // shared-key from config
                var stringToBeHashed = $"{userName}{email}{sharedKey}{day}{month}";
                var url = _config.GetSection("HelpDeskSettings").GetSection("HelpDeskUrl").Value;

                var userHash = PackageTracker.Domain.Utilities.CryptoUtility.CreateHashUsingMD5Async(stringToBeHashed);

                helpDeskAutoLoginUrl = $"{url}/User/AutoLogin?username={userName}&email={email}&userHash={userHash}";
            }

            return helpDeskAutoLoginUrl;
        }

        /// <summary>
        /// Overloaded method that includes additional parameters
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="email"></param>
        /// <param name="inquiryId"></param>
        /// <param name="packageDataSetId"></param>
        /// <param name="validateInputs"></param>
        /// <returns></returns>
        public async Task<string> GenerateHelpDeskUrlAsync(string userName, string email, string secret, string inquiryId,
            string packageId, string shippingTrackingNumber, string siteName, int? packageDatasetId, string carrier, bool validateInputs)
        {
            DateTime date = DateTime.Now;
            string helpDeskAutoLoginUrl = null;
            var day = date.ToString("dd");
            var month = date.ToString("MM");
            var sharedKey = _config.GetSection("HelpDeskSettings").GetSection("shared-key").Value;

            if (secret.Equals(sharedKey))
            {
                // shared-key from config
                var stringToBeHashed = $"{userName}{email}{sharedKey}{day}{month}";
                var url = _config.GetSection("HelpDeskSettings").GetSection("HelpDeskUrl").Value;

                var userHash = PackageTracker.Domain.Utilities.CryptoUtility.CreateHashUsingMD5Async(stringToBeHashed);

                if(inquiryId == null || inquiryId == "Create new inquiry")
                {
                    helpDeskAutoLoginUrl = $"{url}/User/AutoLoginWithInquiryId?username={userName}&email={email}&userHash={userHash}&new_ticket=1"
                        + $"&packageId={packageId}&shippingTrackingNumber={shippingTrackingNumber}&siteName={siteName}"
                        + $"&packageDatasetId={packageDatasetId}&shippingCarrier={carrier}&validateInputs={validateInputs}";
                }
                else
                {
                    helpDeskAutoLoginUrl = $"{url}/User/AutoLoginWithInquiryId?username={userName}&email={email}&userHash={userHash}&inquiryId={inquiryId}";
                }
            }

            return helpDeskAutoLoginUrl;
        }

        public async Task<string> GenerateHelpDeskLogoutURLAsync()
        {
            return $"{_config.GetSection("HelpDeskSettings").GetSection("HelpDeskUrl").Value}/User/Logout";
        }
    }
}
