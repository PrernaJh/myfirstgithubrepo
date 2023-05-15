using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ParcelPrepGov.Web.Features.Dashboard.Data
{
	public static class PackagesData
	{
		public static List<PackageDataset> Execute(IServiceProvider serviceProvider)
		{
			var dbContextFactory = serviceProvider.GetService<IPpgReportsDbContextFactory>();

			if (dbContextFactory is null) throw new ArgumentNullException(nameof(dbContextFactory));

			var context = dbContextFactory.CreateDbContext();

			if (context is null) throw new ArgumentNullException(nameof(context));

			var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

			if (httpContextAccessor is null) throw new ArgumentNullException(nameof(httpContextAccessor));

			var user = httpContextAccessor.HttpContext.User;

			if (user.IsSystemAdministrator() || user.IsAdministrator())
				return context.PackageDatasets.ToList();

			return context.PackageDatasets.Where(x => x.SiteName == user.GetSite()).ToList();
		}
	}
}
