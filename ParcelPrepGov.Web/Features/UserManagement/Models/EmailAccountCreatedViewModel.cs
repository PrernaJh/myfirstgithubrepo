namespace ParcelPrepGov.Web.Features.UserManagement.Models
{
	public class EmailAccountCreatedViewModel
	{
		public string Username { get; private set; }
		public string Email { get; private set; }
		public string ResetPasswordUrl { get; private set; }
		public int ExpirationDays { get; private set; }

		public EmailAccountCreatedViewModel(string username, string email, string resetPasswordUrl, int expirationDays)
		{
			Username = username;
			Email = email;
			ResetPasswordUrl = resetPasswordUrl;
			ExpirationDays = expirationDays;
		}
	}
}
