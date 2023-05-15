namespace PackageTracker.Domain.Models.Account
{
	public class LoginResponse
	{
		public bool Succeeded { get; set; }
		public string AuthorizationTier { get; set; }
		public string Site { get; set; }
		public int ConsecutiveScansAllowed { get; set; }
	}
}
