using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using System.Security.Claims;
using System.Text.Json;

namespace PackageTracker.Identity.Service
{
    public class OktaLocalityClaimAction : ClaimAction
    {
        public OktaLocalityClaimAction() : base(ClaimTypes.Locality, ClaimValueTypes.String) { }

        public override void Run(JsonElement userData, ClaimsIdentity identity, string issuer)
        {
            if (userData.TryGetProperty("address", out var addressProp))
            {
                if (addressProp.TryGetProperty("locality", out var siteName))
                {
                    identity.AddClaim(new Claim(ClaimType, siteName.GetString()));
                }
            }
        }
    }
}
