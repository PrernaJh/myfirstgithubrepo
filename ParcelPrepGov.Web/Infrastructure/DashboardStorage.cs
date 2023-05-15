using DevExpress.DashboardWeb;
using DevExpress.Data.ODataLinq.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Models;
using ParcelPrepGov.Web.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ParcelPrepGov.Web.Infrastructure
{
	public interface IPpgDashboardStorage : IEditableDashboardStorage
	{
		void DeleteDashboard(string dashboardId);
	}

	public class DashboardStorage : IPpgDashboardStorage
	{
		private readonly IPpgReportsDbContextFactory _factory;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public DashboardStorage(IServiceProvider serviceProvider)
		{
			_factory = serviceProvider.GetService<IPpgReportsDbContextFactory>();
			_httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

			if (_factory == null)
				throw new ArgumentNullException(nameof(_factory));

			if (_httpContextAccessor == null)
				throw new ArgumentNullException(nameof(_httpContextAccessor));
		}

		public string AddDashboard(XDocument dashboard, string dashboardName)
		{
			var site = _httpContextAccessor.HttpContext.User.GetSite();
			var username = _httpContextAccessor.HttpContext.User.GetUsername();

			var newDashboard = new Dashboard
			{
				DashboardXml = dashboard.ToString(),
				DashboardName = dashboardName,
				Site = site,
				UserName = username
			};

			using var context = _factory.CreateDbContext();
			context.Add(newDashboard);
			context.SaveChanges();

			return newDashboard.Id.ToString();

		}

		public IEnumerable<DashboardInfo> GetAvailableDashboardsInfo()
		{
			var site = _httpContextAccessor.HttpContext.User.GetSite();
			var username = _httpContextAccessor.HttpContext.User.GetUsername();

			using var context = _factory.CreateDbContext();
			var dashboards = context.Dashboards.Where(x => x.Site == site & x.UserName == username).ToList();

			List<DashboardInfo> dashboardInfos = new List<DashboardInfo>();

			foreach (var dashboard in dashboards)
			{
				DashboardInfo dashboardInfo = new DashboardInfo
				{
					ID = dashboard.Id.ToString(),
					Name = dashboard.DashboardName
				};
				dashboardInfos.Add(dashboardInfo);
			}

			return dashboardInfos;
		}

		public XDocument LoadDashboard(string dashboardID)
		{
			using var context = _factory.CreateDbContext();
			var dashboard = context.Dashboards.FirstOrDefault(x => x.Id == int.Parse(dashboardID));
			return dashboard.DashboardXml == null ? null : XDocument.Parse(dashboard.DashboardXml.ToString());
		}

		public void SaveDashboard(string dashboardID, XDocument dashboard)
		{
			using var context = _factory.CreateDbContext();
			var existingDashboard = context.Dashboards.FirstOrDefault(x => x.Id == int.Parse(dashboardID));
			existingDashboard.DashboardXml = dashboard.ToString();
			context.Update(existingDashboard);
			context.SaveChanges();
		}

		public void DeleteDashboard(string dashboardId)
		{
			using var context = _factory.CreateDbContext();
			var dashboard = context.Dashboards.FirstOrDefault(x => x.Id == int.Parse(dashboardId));
			if (dashboard == null) return;

			context.Remove(dashboard);
			context.SaveChanges();
		}
	}
}
