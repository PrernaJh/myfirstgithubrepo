using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using System.Security.Claims;
using System.Text.Json;

namespace PackageTracker.Identity.Service
{
    public class OktaGroupArrayClaimAction : ClaimAction
    {
        public OktaGroupArrayClaimAction() : base(ClaimTypes.Role, ClaimValueTypes.String) { }

        public override void Run(JsonElement userData, ClaimsIdentity identity, string issuer)
        {
            if (userData.TryGetProperty("groups", out var groupArrayProp))
            {
                foreach (var group in groupArrayProp.EnumerateArray())
                {
                    identity.AddClaim(new Claim(ClaimType, group.GetString()));
                }
            }
        }
    }
}
