using MMS.API.Domain.Models;

namespace PackageTracker.Domain.Models
{
    public class ValidateTokenRequest : BaseRequest
    {
        public string SubClient { get; set; }
    }
}
