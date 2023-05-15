namespace PackageTracker.Communications.Models
{
	public class SmsResponse
	{
		public string Status { get; }
		public int? ErrorCode { get; }
		public string ErrorMessage { get; }

		public SmsResponse(string status, int? errorCode, string errorMessage)
		{
			Status = status;
			ErrorCode = errorCode;
			ErrorMessage = errorMessage;
		}

	}
}
