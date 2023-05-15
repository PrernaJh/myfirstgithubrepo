using Moq;
using PackageTracker.Data.Interfaces;
using PackageTracker.Data.Models;
using System.Threading.Tasks;
using Xunit;

namespace PackageTracker.Domain.Tests
{
    public class SiteProcessorTests
    {
        [Fact]
        public async Task GetSiteCriticalEmailListBySiteId_ValidId_ReturnsList()
        {
            var mockSiteRepository = new Mock<ISiteRepository>();
            var validSiteId = "123456789";

            mockSiteRepository.Setup(repo => repo.GetItemAsync(It.Is<string>(id => id == validSiteId), It.IsAny<string>()))
                .ReturnsAsync(new Site());

            var siteProcessor = new SiteProcessor(mockSiteRepository.Object);

            var critialEmailListForSite = await siteProcessor.GetSiteCritialEmailListBySiteId(validSiteId);

            Assert.NotNull(critialEmailListForSite);
        }

        [Fact]
        public async Task GetSiteCriticalEmailListBySiteId_InvalidId_ReturnsEmptyList()
        {
            var invalidSiteId = "12345689";
            var mockSiteRepository = new Mock<ISiteRepository>();

            mockSiteRepository.Setup(repo => repo.GetItemAsync(It.Is<string>(id => id == invalidSiteId), It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var siteProcessor = new SiteProcessor(mockSiteRepository.Object);

            var emptyCritialEmailListForSite = await siteProcessor.GetSiteCritialEmailListBySiteId(invalidSiteId);

            Assert.Empty(emptyCritialEmailListForSite);
        }

        [Fact]
        public async Task GetSiteCriticalEmailListBySiteName_ValidSiteName_ReturnsList()
        {
            var mockSiteRepository = new Mock<ISiteRepository>();
            var validSiteName = "CHARLESTON";

            mockSiteRepository.Setup(repo => repo.GetSiteBySiteNameAsync(It.Is<string>(siteName => siteName == validSiteName)))
                .ReturnsAsync(new Site());

            var siteProcessor = new SiteProcessor(mockSiteRepository.Object);

            var criticalEmailListForSite = await siteProcessor.GetSiteCritialEmailListBySiteName(validSiteName);

            Assert.NotNull(criticalEmailListForSite);
        }

        [Fact]
        public async Task GetSiteCriticalEmailListBySiteName_InvalidSiteName_ReturnsEmptyList()
        {
            var invalidSiteName = "blah";
            var mockSiteRepository = new Mock<ISiteRepository>();

            mockSiteRepository.Setup(repo => repo.GetSiteBySiteNameAsync(It.Is<string>(siteName => siteName == invalidSiteName)))
                .ReturnsAsync(() => null);

            var siteProcessor = new SiteProcessor(mockSiteRepository.Object);

            var criticalEmailListForSite = await siteProcessor.GetSiteCritialEmailListBySiteName(invalidSiteName);

            Assert.Empty(criticalEmailListForSite);
        }
    }
}
