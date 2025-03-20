using System.IO;
using System.Text;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Moq;
using AdPlatformsService;

namespace AdPlatformsService.Tests
{
    [TestFixture]
    public class AdsControllerUploadTests
    {
        private AdPlatformsController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new AdPlatformsController();
        }

        [Test]
        public void UploadAdPlatforms_ValidFile_ShouldLoadDataCorrectly()
        {
            // Arrange
            var fileContent = "������.������:/ru\n���������� �������:/ru/svrd/revda,/ru/svrd/pervik";
            var mockFile = new Mock<IFormFile>();
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            mockFile.Setup(f => f.Length).Returns(memoryStream.Length);

            // Act
            var result = _controller.UploadAdPlatforms(mockFile.Object);

            // Assert
            Assert.That(result, Is.InstanceOf<OkResult>());

            var data = AdPlatformsController.GetDataForTesting();
            Assert.That(data, Has.Count.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(data["/ru"], Does.Contain("������.������"));
                Assert.That(data["/ru/svrd/revda"], Does.Contain("���������� �������"));
                Assert.That(data["/ru/svrd/pervik"], Does.Contain("���������� �������"));
            });
        }

        [Test]
        public void UploadAdPlatforms_EmptyFile_ShouldReturnBadRequest()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            // Act
            var result = _controller.UploadAdPlatforms(mockFile.Object);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("File is required."));
        }

        [Test]
        public void UploadAdPlatforms_InvalidFileFormat_ShouldIgnoreInvalidLines()
        {
            // Arrange
            var fileContent = "������.������:/ru\nInvalidLine\n���������� �������:/ru/svrd/revda";
            var mockFile = new Mock<IFormFile>();
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            mockFile.Setup(f => f.Length).Returns(memoryStream.Length);

            // Act
            var result = _controller.UploadAdPlatforms(mockFile.Object);

            // Assert
            Assert.That(result, Is.InstanceOf<OkResult>());

            var data = AdPlatformsController.GetDataForTesting();
            Assert.That(data, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(data["/ru"], Does.Contain("������.������"));
                Assert.That(data["/ru/svrd/revda"], Does.Contain("���������� �������"));
            });
        }
    }
}