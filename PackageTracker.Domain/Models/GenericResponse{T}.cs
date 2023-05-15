namespace PackageTracker.Domain.Models
{
	public class GenericResponse<T>
	{
		public GenericResponse(T responseData, bool success = true, string message = "")
		{
			Data = responseData;
			Success = success;
			Message = message;
		}

		public bool Success { get; }
		public string Message { get; }
		public T Data { get; }
	}
}