using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace ParcelPrepGov.Web.Models
{
	public class ApiError
	{
		public bool Success { get; }
		public string Message { get; set; }
		public string Detail { get; set; }

		public ApiError()
		{
		}

		public ApiError(string message)
		{
			Message = message;
		}

		public ApiError(ModelStateDictionary modelState)
		{
			Message = "Invalid parameters.";
			Detail = modelState
				.FirstOrDefault(x => x.Value.Errors.Any()).Value.Errors
				.FirstOrDefault().ErrorMessage;
		}
	}
}
