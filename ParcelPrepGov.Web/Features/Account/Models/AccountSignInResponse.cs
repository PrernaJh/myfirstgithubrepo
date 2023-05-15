namespace ParcelPrepGov.Web.Features.Account.Models
{
	public class AccountSignInResponse
	{
		public bool Success { get; }
		public string Message { get; }
		public string RedirectUrl { get; set; }
		public string Token { get; }

		public AccountSignInResponse(bool success, string message, string redirectUrl, string token)
		{
			Success = success;
			Message = message;
			RedirectUrl = redirectUrl;
			Token = token;
		}
	}
}

