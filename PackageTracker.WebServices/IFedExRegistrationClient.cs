using FedExRegistrationApi;
using System.Threading.Tasks;

namespace PackageTracker.WebServices
{
    public interface IFedExRegistrationClient
    {
        Task<registerWebUserResponse> registerWebUserAsync(RegisterWebUserRequest RegisterWebUserRequest);
        Task<subscriptionResponse> subscriptionAsync(SubscriptionRequest SubscriptionRequest);
    }
}
