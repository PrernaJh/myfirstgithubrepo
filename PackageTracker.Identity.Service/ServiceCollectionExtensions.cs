using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using PackageTracker.Identity.Data;
using PackageTracker.Identity.Data.Models;
using PackageTracker.Identity.Service.Configuration;
using PackageTracker.Identity.Service.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;

namespace PackageTracker.Identity.Service
{
    public static class ServiceCollectionExtensions
	{
		public static IServiceCollection BootstrapIdentityService(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
		{
			services.Configure<JwtConfiguration>(configuration.GetSection(JwtConfiguration.JwtSection));

			services.AddDbContext<PackageTrackerIdentityDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("IdentityDb")));

			
			services.AddIdentity<ApplicationUser, IdentityRole>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = true;
				options.Password.RequireLowercase = true;
			})
				.AddEntityFrameworkStores<PackageTrackerIdentityDbContext>()	
				.AddPasswordValidator<PPGPasswordValidator<ApplicationUser>>()
				.AddDefaultTokenProviders();

			services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AddUserClaimsFactory>();

			services.AddScoped<IExternalSignInManager, FedexOktaExternalSignInManager>();

			//When we use the Authorize attribute, it actually binds to the first authentication system by default. 
			//The trick is to change the attribute to specify which auth to use:
			//Default: [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
			//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
			//[Authorize(AuthenticationSchemes = AuthSchemes)]
			services.AddAuthentication()
				.AddJwtBearer(cfg =>
				{
					cfg.RequireHttpsMetadata = true;
					cfg.SaveToken = true;

					cfg.TokenValidationParameters = new TokenValidationParameters()
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateIssuerSigningKey = true,
						ValidateLifetime = true,
						ValidIssuer = configuration["JwtSettings:Issuer"],
						ValidAudience = configuration["JwtSettings:Audience"],
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"])),
					};
				})
				.AddOpenIdConnect(cfg =>
				{
					cfg.Authority = configuration["OpenIdConnect:Okta:Authority"];
					cfg.ClientId = configuration["OpenIdConnect:Okta:ClientId"];
					cfg.ClientSecret = configuration["OpenIdConnect:Okta:ClientSecret"];

					cfg.ClaimActions.Add(new OktaGroupArrayClaimAction());
					cfg.ClaimActions.Add(new OktaLocalityClaimAction());
					cfg.GetClaimsFromUserInfoEndpoint = true;
					cfg.ResponseType = OpenIdConnectResponseType.Code;
					cfg.SaveTokens = true;
					cfg.Scope.Add("address");
					cfg.Scope.Add("groups");
                });

			services.ConfigureApplicationCookie(options =>
			{
				options.Cookie.Name = "ppgpro_web";
				options.Cookie.HttpOnly = true;
				options.Cookie.SecurePolicy = env.IsDevelopment() ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
				options.Cookie.IsEssential = true;
				options.LoginPath = $"/Account/SignIn";
				options.AccessDeniedPath = $"/Account/AccessDenied";
				options.LogoutPath = $"/Account/SignOut";				
				options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
				options.Events.OnSigningIn = ctx =>
				{
					if (ctx.Properties.ExpiresUtc.HasValue)
						ctx.HttpContext.Response.Cookies.AddLoginExpirationCookie(ctx.Properties.ExpiresUtc.Value);

					return Task.FromResult(0);
				};
			});


			services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.FromSeconds(60));

			services.Configure<SingleSignOnOptions>(configuration);

			services.AddScoped<IIdentityService, PPGIdentityService>();

			return services;
		}

		public static void AddLoginExpirationCookie(this IResponseCookies cookies, DateTimeOffset expiration)
		{
			cookies.Append(
					"LoginExpiration",
					expiration.ToString(),
					new CookieOptions()
					{
						Path = "/",
						IsEssential = true,
						Expires = expiration
					}
				); 
		}
	}
}
