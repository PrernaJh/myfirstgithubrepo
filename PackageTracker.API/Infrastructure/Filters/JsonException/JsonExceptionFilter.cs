using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;

namespace PackageTracker.API.Infrastructure.Filters.JsonException
{
	public class JsonExceptionFilter : IExceptionFilter
	{
		private readonly IWebHostEnvironment env;
		private readonly TelemetryClient telemetryClient;

		public JsonExceptionFilter(IWebHostEnvironment env, TelemetryClient telemetryClient)
		{
			this.env = env;
			this.telemetryClient = telemetryClient;
		}

		public void OnException(ExceptionContext context)
		{
			var error = new ApiError();

			if (env.IsDevelopment())
			{
				error.Message = context.Exception.Message;
				error.Detail = context.Exception.StackTrace;
			}
			else
			{
				error.Message = "Sorry, server error occurred.";
				error.Detail = context.Exception.Message;
				telemetryClient.TrackException(context.Exception);
				telemetryClient.Flush();
			}

			context.Result = new ObjectResult(error)
			{
				StatusCode = 500
			};
		}
	}
}