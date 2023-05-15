using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Infrastructure.Middleware
{
	public class ValidateAntiForgeryTokenMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IAntiforgery _antiforgery;

		public ValidateAntiForgeryTokenMiddleware(RequestDelegate next, IAntiforgery antiforgery)
		{
			_next = next;
			_antiforgery = antiforgery;
		}

		public async Task Invoke(HttpContext context)
		{
			if (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method))
			{
				if (!context.Request.Path.StartsWithSegments(new PathString("/api/dashboards")))
					await _antiforgery.ValidateRequestAsync(context);
			}

			await _next(context);
		}
	}

	public static partial class ApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseAntiforgeryTokens(this IApplicationBuilder app)
		{
			return app.UseMiddleware<ValidateAntiForgeryTokenMiddleware>();
		}
	}
}
