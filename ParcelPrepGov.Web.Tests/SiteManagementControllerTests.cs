using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PackageTracker.Data.Models;
using PackageTracker.Domain.Interfaces;
using PackageTracker.Domain.Models;
using ParcelPrepGov.Web.Features.UserManagement;
using ParcelPrepGov.Web.Features.UserManagement.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace ParcelPrepGov.Web.Tests
{
	public class SiteManagementControllerTests
	{
		[Fact]
		public async Task GetCriticalEmailListBySiteId_ValidId_ReturnsJsonResultWithCriticalEmailModelList()
        {
			var mockSiteProcessor = new Mock<ISiteProcessor>();
			var mockLogger = new Mock<ILogger<SiteManagementController>>();
			var siteId = "123456789";
			var testEmail = "test@tecmailing.com";

			mockSiteProcessor.Setup(processor => processor.GetSiteByIdAsync(It.Is<string>(id => id == siteId)))
				.ReturnsAsync(new Site()
				{
					SiteName = "CHARLESTON",
					CriticalAlertEmailList = new List<string>() { testEmail }
                });

			var controller = new SiteManagementController(mockSiteProcessor.Object, mockLogger.Object);

			var result = await controller.GetCriticalEmailListBySiteId(siteId);

			var jsonResult = result as JsonResult;

			var jsonDataList = jsonResult.Value as IEnumerable<CriticalEmailModel>;

			Assert.Contains(jsonDataList, model => model.Email == testEmail);
        }

		[Fact]
		public async Task GetCriticalEmailListBySiteName_ValidSiteName_ReturnsJsonResultWithCriticalEmailModelList()
        {
			var mockSiteProcessor = new Mock<ISiteProcessor>();
			var mockLogger = new Mock<ILogger<SiteManagementController>>();
			var siteName = "CHARLESTON";
			var testEmail = "test@tecmailing.com";

			mockSiteProcessor.Setup(processor => processor.GetSiteCritialEmailListBySiteName(It.Is<string>(name => name == siteName)))
				.ReturnsAsync(new List<string>()
				{
					testEmail
				});

			var controller = new SiteManagementController(mockSiteProcessor.Object,mockLogger.Object);

			var result = await controller.GetCriticalEmailListBySiteName(siteName);

			var jsonResult = result as JsonResult;

			var jsonDataList = jsonResult.Value as IEnumerable<CriticalEmailModel>;

			Assert.Contains(jsonDataList, model => model.Email == testEmail);
		}

		[Fact]
		public async Task AddCriticalEmailToList_AnySiteAndEmail_ReturnsJsonWithGenericResponseData()
        {
			var emailToAdd = "test@tecmailing.com";
			var mockSiteProcessor = new Mock<ISiteProcessor>();
			var mockLogger = new Mock<ILogger<SiteManagementController>>();
			var requestBody = new CriticalEmailModel()
			{
				SiteName = "CHARLESTON",
				Email = emailToAdd
			};

			mockSiteProcessor.Setup(processor => processor.AddUserToCriticalEmailList(It.Is<ModifyCritialEmailListRequest>(request =>
				request.SiteName == requestBody.SiteName && request.Email == requestBody.Email
			))).ReturnsAsync(new GenericResponse<string>(emailToAdd));

			var controller = new SiteManagementController(mockSiteProcessor.Object, mockLogger.Object);

            var result = await controller.AddCriticalEmailToList(JsonSerializer.Serialize(requestBody));

            var jsonResult = result as JsonResult;

            var jsonDataGenericResponse = jsonResult.Value as GenericResponse<string>;

            Assert.True(jsonDataGenericResponse.Success);
        }

        [Fact]
		public void RemoveCriticalEmailFromList_AnySiteAndEmail_MethodForRemovingEmailFromListIsCalled()
        {
			var emailToRemove = "test@tecmailing.com";
			var mockSiteProcessor = new Mock<ISiteProcessor>();
			var mockLogger = new Mock<ILogger<SiteManagementController>>();
			var requestBody = new CriticalEmailModel()
			{
				SiteName = "CHARLESTON",
				Email = emailToRemove
			};

			mockSiteProcessor.Setup(processor => processor.RemoveUserFromCriticalEmailList(It.Is<ModifyCritialEmailListRequest>(request =>
				request.SiteName == requestBody.SiteName && request.Email == requestBody.Email
			))).ReturnsAsync(new GenericResponse<string>(emailToRemove)).Verifiable();

			var controller = new SiteManagementController(mockSiteProcessor.Object, mockLogger.Object);

            controller.RemoveCriticalEmailFromList(JsonSerializer.Serialize(requestBody));

			mockSiteProcessor.Verify();
        }
    }
}
