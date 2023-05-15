using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PackageTracker.Domain.Interfaces;
using ParcelPrepGov.Reports.Data;
using ParcelPrepGov.Reports.Interfaces;
using ParcelPrepGov.Reports.Repositories;
using ParcelPrepGov.Web.Features.ServiceManagement;
using System.Collections.Generic;
using Xunit;

namespace ParcelPrepGov.Web.Tests
{
    public class ManageUspsRegionsControllerTests
    {
		private readonly Mock<ISiteProcessor> mockSiteProcessor;
		private readonly Mock<ISubClientProcessor> mockSubClientProcessor;
		private readonly Mock<IServiceRuleProcessor> mockServiceRuleProcessor;
		private readonly Mock<IBinFileProcessor> mockBinFileProcessor;
		private readonly Mock<IZipOverrideProcessor> mockZipOverrideProcessor;
		private readonly Mock<IRateProcessor> mockRateProcessor;
		private readonly Mock<IRateFileProcessor> mockRateFileProcessor;
		private readonly Mock<IContainerRateFileProcessor> mockContainerRateFileProcessor;
		private readonly Mock<IActiveGroupProcessor> mockActiveGroupProcessor;
		private readonly Mock<IMapper> mockMapper;
		private readonly Mock<IWebHostEnvironment> mockEnvironment;
        private readonly Mock<IPostalAreaAndDistrictRepository> mockPostalRepository;
        private readonly Mock<IWebJobRunProcessor> mockWebJobRunProcessor;
		private readonly Mock<ILogger<ServiceManagementController>> mockLogger;
        private readonly Mock<IServiceRuleExtensionFileProcessor> mockServiceRuleFileProcessor;
        private readonly Mock<IServiceRuleExtensionProcessor> mockserviceRuleExtension;
        private readonly Mock<IPostalDaysRepository> mockHolidaysRepository;
        private readonly Mock<IEvsCodeRepository> mockEvsCodeRepository;
        private readonly Mock<IVisnSiteRepository> mockVisnSiteRepository;

        public ManageUspsRegionsControllerTests()
        {
            mockSiteProcessor = new Mock<ISiteProcessor>();
            mockSubClientProcessor = new Mock<ISubClientProcessor>();
            mockserviceRuleExtension = new Mock<IServiceRuleExtensionProcessor>();
            mockServiceRuleProcessor = new Mock<IServiceRuleProcessor>();
            mockServiceRuleFileProcessor = new Mock<IServiceRuleExtensionFileProcessor>();
            mockBinFileProcessor = new Mock<IBinFileProcessor>();
            mockZipOverrideProcessor = new Mock<IZipOverrideProcessor>();
            mockRateFileProcessor = new Mock<IRateFileProcessor>();
            mockContainerRateFileProcessor = new Mock<IContainerRateFileProcessor>();
            mockActiveGroupProcessor = new Mock<IActiveGroupProcessor>();
            mockMapper = new Mock<IMapper>();
            mockEnvironment = new Mock<IWebHostEnvironment>();
            mockRateProcessor = new Mock<IRateProcessor>();
            mockPostalRepository = new Mock<IPostalAreaAndDistrictRepository>();
            mockWebJobRunProcessor = new Mock<IWebJobRunProcessor>();
            mockHolidaysRepository = new Mock<IPostalDaysRepository>();
            mockEvsCodeRepository = new Mock<IEvsCodeRepository>();
            mockLogger = new Mock<ILogger<ServiceManagementController>>();
            mockVisnSiteRepository = new Mock<IVisnSiteRepository>();
        }

        [Fact]
        public void Index_CallMethod_ReturnsCorrectView()
        {
            var controller = new ServiceManagementController(mockEnvironment.Object,
                mockMapper.Object, 
                mockSiteProcessor.Object,
                mockSubClientProcessor.Object, 
                mockServiceRuleProcessor.Object,
                mockServiceRuleFileProcessor.Object,
                mockserviceRuleExtension.Object,
                mockBinFileProcessor.Object, 
                mockZipOverrideProcessor.Object, 
                mockRateProcessor.Object, 
                mockContainerRateFileProcessor.Object,
                mockRateFileProcessor.Object, 
                mockActiveGroupProcessor.Object,
                mockPostalRepository.Object,
                mockWebJobRunProcessor.Object,
                mockHolidaysRepository.Object,
                mockEvsCodeRepository.Object,
                mockVisnSiteRepository.Object,
                mockLogger.Object);
            var correctViewName = "ManageUspsRegions";
            var result = controller.ManageUspsRegions();
            var viewResult = result as ViewResult;

            Assert.Equal(correctViewName, viewResult.ViewName);
        }

        [Fact]
        public void Test_DatasetRepository()
        {
            var creators = new Mock<List<DatasetRepository>>();
            Mock<ILogger<PostalAreaAndDistrictRepository>> logger = new Mock<ILogger<PostalAreaAndDistrictRepository>>();
            Mock<ILogger<PostalDaysRepository>> holdayLogger = new Mock<ILogger<PostalDaysRepository>>();
            Mock<IBlobHelper> blobHelper = new Mock<IBlobHelper>();
             
            Mock<System.Data.IDbConnection> connection = new Mock<System.Data.IDbConnection>();
            Mock<IPpgReportsDbContextFactory> factory = new Mock<IPpgReportsDbContextFactory>();
            
            // fails to mock dbContext, required for bulkInsert
            // moving on to implementation
            var mockDbContext = new Mock<PpgReportsDbContext>();
            factory.Setup(f => f.CreateDbContext()).Returns(mockDbContext.Object);
            
            var config = new Mock<IConfiguration>();
            // required for timeout mock config
            var mockTimeout = new Mock<IConfigurationSection>();
            mockTimeout.Setup(s => s.Key).Returns("SqlCommandTimeout");
            mockTimeout.Setup(s => s.Value).Returns("300");

            config.Setup(config => config.GetSection("SqlCommandTimeout")).
              Returns(mockTimeout.Object);

            // add the repositories to be executed            
            creators.Object.Add(new PostalAreaAndDistrictRepository(logger.Object,blobHelper.Object, config.Object, connection.Object, factory.Object));
            creators.Object.Add(new PostalDaysRepository(holdayLogger.Object, blobHelper.Object, config.Object, connection.Object, factory.Object));

            foreach (DatasetRepository item in creators.Object)
            {
                var itemList = new List<string>();
                // we have access to datasetRepository methods
                // if we want access to other methods we would 
                // have to have a common base interface or abstract class
                // see test below
                item.ExecuteBulkInsertAsync(itemList).GetAwaiter().GetResult();
                Assert.NotNull(item);
            }
        }

        /// <summary>
        /// mocked request to build out repositories
        /// </summary>
        [Fact]
        public void Test_Interface_FileUpload()
        {
            var creators = new Mock<List<IFileManager>>();
            Mock<ILogger<PostalAreaAndDistrictRepository>> logger = new Mock<ILogger<PostalAreaAndDistrictRepository>>();
            Mock<ILogger<PostalDaysRepository>> holdayLogger = new Mock<ILogger<PostalDaysRepository>>();
            Mock<ILogger<VisnSiteRepository>> visnLogger = new Mock<ILogger<VisnSiteRepository>>();
            Mock<ILogger<EvsCodeRepository>> evsLogger = new Mock<ILogger<EvsCodeRepository>>();
            Mock<IBlobHelper> blobHelper = new Mock<IBlobHelper>();
             
            var config = new Mock<IConfiguration>();
            var mockTimeout = new Mock<IConfigurationSection>();
            mockTimeout.Setup(s => s.Key).Returns("SqlCommandTimeout");
            mockTimeout.Setup(s => s.Value).Returns("300");

            var mockSectionImportFile = new Mock<IConfigurationSection>();
            mockSectionImportFile.Setup(s => s.Key).Returns("UspsFileImportArchive");
            mockSectionImportFile.Setup(s => s.Value).Returns("uspsfileimport-archive");

            config.Setup(x => x.GetSection("SqlCommandTimeout")).
              Returns(mockTimeout.Object);
            config.Setup(x => x.GetSection("UspsFileImportArchive")).
             Returns(mockSectionImportFile.Object); 

            Mock<System.Data.IDbConnection> connection = new Mock<System.Data.IDbConnection>();
            Mock<IPpgReportsDbContextFactory> factory = new Mock<IPpgReportsDbContextFactory>();

            // add the repositories to be executed            
            creators.Object.Add(new PostalAreaAndDistrictRepository(logger.Object, blobHelper.Object, config.Object, connection.Object, factory.Object));
            creators.Object.Add(new PostalDaysRepository(holdayLogger.Object, blobHelper.Object, config.Object, connection.Object, factory.Object));
            creators.Object.Add(new VisnSiteRepository(visnLogger.Object, blobHelper.Object, config.Object, connection.Object, factory.Object));
            creators.Object.Add(new EvsCodeRepository(evsLogger.Object, blobHelper.Object, config.Object, connection.Object, factory.Object));
            
            foreach (var item in creators.Object)
            {
                var foundFile = item.DownloadFileAsync("padd.xlsx").GetAwaiter().GetResult();
                Assert.NotNull(foundFile);
            }
        }
    }
}
