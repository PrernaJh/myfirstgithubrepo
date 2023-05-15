using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PackageTracker.Identity.Service;
using System;
using System.Threading.Tasks;

namespace ParcelPrepGov.Web.Infrastructure.Middleware
{
    public class LoginExpirationCookieMiddleware
    {
        private readonly RequestDelegate _next;

		public LoginExpirationCookieMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			var identityContext = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);

			if (identityContext?.Properties?.ExpiresUtc != null)
			{
				context.Response.Cookies.AddLoginExpirationCookie(identityContext.Properties.ExpiresUtc.Value);
			}

			await _next(context);
		}
	}

	public static partial class ApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseLoginExpirationCookie(this IApplicationBuilder app)
		{
			return app.UseMiddleware<LoginExpirationCookieMiddleware>();
		}
	}
}
