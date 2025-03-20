using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;

namespace AdPlatformsService.Tests
{
    [TestFixture]
    public class AdsControllerGetTests
    {
        private AdPlatformsController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new AdPlatformsController();

            // Test data setup
            var testData = new Dictionary<string, ImmutableList<string>>
            {
                { "/ru", ImmutableList.Create("Яндекс.Директ") },
                { "/ru/svrd", ImmutableList.Create("Крутая реклама") },
                { "/ru/svrd/revda", ImmutableList.Create("Ревдинский рабочий") }
            };

            AdPlatformsController.SetDataForTesting(testData.ToImmutableDictionary());
        }

        [Test]
        public void GetAdPlatforms_ValidLocation_ShouldReturnCorrectAds()
        {
            // Arrange
            var location = "/ru/svrd/revda";

            // Act
            var result = _controller.GetAdPlatforms(location);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var ads = okResult.Value as List<string>;

            Assert.That(ads, Has.Count.EqualTo(3));
            Assert.That(ads, Does.Contain("Яндекс.Директ"));
            Assert.That(ads, Does.Contain("Крутая реклама"));
            Assert.That(ads, Does.Contain("Ревдинский рабочий"));
        }

        [Test]
        public void GetAdPlatforms_InvalidLocation_ShouldReturnEmptyList()
        {
            // Arrange
            var location = "/invalid/location";

            // Act
            var result = _controller.GetAdPlatforms(location);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            var ads = okResult.Value as List<string>;

            Assert.That(ads, Is.Empty);
        }

        [Test]
        public void GetAdPlatforms_EmptyLocation_ShouldReturnBadRequest()
        {
            // Arrange
            var location = "";

            // Act
            var result = _controller.GetAdPlatforms(location);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("Location is required."));
        }
    }
}