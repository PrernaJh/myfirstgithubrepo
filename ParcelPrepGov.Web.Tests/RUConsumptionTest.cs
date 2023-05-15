using System;
using System.Collections.Generic;
using System.Text;
//using Microsoft.Azure.WebJobs;
//using PackageTracker.Service;
using Xunit;

namespace ParcelPrepGov.Web.Tests
{
    public class RUConsumptionTest
    {
        [Fact]
        void Test_Insert_Blank_Document()
        {
            // Arrange
            var mockWebJobManager = new Moq.Mock<IWebJobManager>();
            var timer = new Moq.Mock<ITimerInfo>();

            // Act
            var response = mockWebJobManager.Object.CreateEodPackagesJob(timer.Object);

            // Assert
            Assert.Null(response);
        }

        public interface IWebJobManager
        {
            object CreateEodPackagesJob(object @object);
        }

        public interface ITimerInfo
        {
        }
    }
}
