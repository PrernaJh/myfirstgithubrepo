using FedExRegistrationApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PackageTracker.Data.Utilities;
using PackageTracker.Domain.Utilities;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

#if RELEASE
using Microsoft.AspNetCore.Authorization;
using PackageTracker.Identity.Data.Constants;
#endif

namespace PackageTracker.API.Controllers
{    public class RegisterRequest
    {
        public string EndpointUri { get; set; }
        public RegisterWebUserRequest Request { get; set; }
    }

    public class RegisterReply
    {
        public WebAuthenticationCredential UserCredential { get; set; }
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
    }

    public class SubscribeRequest
    {
        public string EndpointUri { get; set; }
        public AESKey Crypto { get; set; }
        public SubscriptionRequest Request { get; set; }
    }

    public class FedExCredentials
    {
        public string ApiKey { get; set; }
        public string ApiPassword { get; set; }
        public string AccountNumber { get; set; }
        public string MeterNumber { get; set; }
    }

    public class SubscribeReply
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public FedExCredentials FedExCredentials { get; set; }
    }

#if RELEASE
    [Authorize(Roles = IdentityDataConstants.SystemAdministrator)]
#endif
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FedExRegistrationController : ControllerBase
    {
        private readonly ILogger<FedExRegistrationController> logger;

        public FedExRegistrationController(ILogger<FedExRegistrationController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        public async Task<RegisterReply> Register([FromBody] RegisterRequest request)
        {
            var reply = new RegisterReply() { IsSuccessful = true };
            try
            {
                var fedExRegistrationClient = new RegistrationPortTypeClient(0, request.EndpointUri);
                var registerRequest = request.Request;
                registerRequest.Version = new VersionId()
                {
                    ServiceId = "fcas",
                    Major = 7,
                    Intermediate = 0,
                    Minor = 0
                };
                registerRequest.Categories = new WebServiceCategoryType[] { WebServiceCategoryType.SHIPPING };
                logger.LogInformation(XmlUtility<RegisterWebUserRequest>.Serialize(registerRequest));
                var registerResponse = await fedExRegistrationClient.registerWebUserAsync(registerRequest);
                logger.LogInformation(XmlUtility<registerWebUserResponse>.Serialize(registerResponse));
                reply.IsSuccessful = registerResponse.RegisterWebUserReply.HighestSeverity == NotificationSeverityType.SUCCESS;
                if (registerResponse.RegisterWebUserReply.Notifications.Length > 0)
                    reply.Message = registerResponse.RegisterWebUserReply.Notifications[0].Message;
                if (reply.IsSuccessful)
                {
                    reply.UserCredential = registerResponse.RegisterWebUserReply.UserCredential;
                }
            }
            catch (Exception ex)
            {
                reply.Message = ex.Message;
                reply.IsSuccessful = false;
            }
            return reply;
        }

        [HttpPost]
        public async Task<SubscribeReply> Subscribe([FromBody] SubscribeRequest request)
        {
            var reply = new SubscribeReply() { IsSuccessful = true };
            try
            {
                var fedExRegistrationClient = new RegistrationPortTypeClient(0, request.EndpointUri);
                var SubscriptionRequest = request.Request;
                SubscriptionRequest.Version = new VersionId()
                {
                    ServiceId = "fcas",
                    Major = 7,
                    Intermediate = 0,
                    Minor = 0
                };
                SubscriptionRequest.CspType = CspType.TRADITIONAL_API;
                SubscriptionRequest.CspTypeSpecified = true;

                logger.LogInformation(XmlUtility<SubscriptionRequest>.Serialize(SubscriptionRequest));
                var subscriptionResponse = await fedExRegistrationClient.subscriptionAsync(SubscriptionRequest);
                logger.LogInformation(XmlUtility<subscriptionResponse>.Serialize(subscriptionResponse));
                reply.IsSuccessful = subscriptionResponse.SubscriptionReply.HighestSeverity == NotificationSeverityType.SUCCESS;
                if (subscriptionResponse.SubscriptionReply.Notifications.Length > 0)
                    reply.Message = subscriptionResponse.SubscriptionReply.Notifications[0].Message;
                if (reply.IsSuccessful)
                {
                    reply.FedExCredentials = new FedExCredentials
                    {
                        ApiKey = CryptoUtility.Encrypt(request.Crypto, request.Request.WebAuthenticationDetail.UserCredential.Key),
                        ApiPassword = CryptoUtility.Encrypt(request.Crypto, request.Request.WebAuthenticationDetail.UserCredential.Password),
                        AccountNumber = request.Request.ClientDetail.AccountNumber,
                        MeterNumber = subscriptionResponse.SubscriptionReply.MeterDetail.MeterNumber
                    };
                }
            }
            catch (Exception ex)
            {
                reply.Message = ex.Message;
                reply.IsSuccessful = false;
            }
            return reply;
        }

    }
}
