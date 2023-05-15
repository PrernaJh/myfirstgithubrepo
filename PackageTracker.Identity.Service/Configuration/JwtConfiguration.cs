namespace PackageTracker.Identity.Service.Configuration
{
	public class JwtConfiguration
	{
		public const string JwtSection = "JwtSettings";

		public string Key { get; set; }
		public int MinutesToExpiration { get; set; }
		public string Issuer { get; set; }
		public string Audience { get; set; }
	}
}
