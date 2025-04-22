using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using BadProject.Infrastructure;
using BadProject.Tests.TestHelpers;
using ThirdParty;

namespace BadProject.Tests.Infrastructure
{
    public class MemoryAdvertisementCacheTests : TestFixture
    {
        private readonly Mock<ILogger<MemoryAdvertisementCache>> _logger;

        public MemoryAdvertisementCacheTests()
        {
            _logger = CreateLogger<MemoryAdvertisementCache>();
        }

        [Fact]
        public async Task GetAsync_WhenItemNotInCache_ReturnsNull()
        {
            // Arrange
            var cache = new MemoryAdvertisementCache(_logger.Object);

            // Act
            var result = await cache.GetAsync("non-existent-id");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_WhenItemInCache_ReturnsItem()
        {
            // Arrange
            var cache = new MemoryAdvertisementCache(_logger.Object);
            var advertisement = CreateTestAdvertisement();
            await cache.SetAsync(advertisement.WebId, advertisement, TimeSpan.FromMinutes(1));

            // Act
            var result = await cache.GetAsync(advertisement.WebId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(advertisement.WebId, result.WebId);
            Assert.Equal(advertisement.Name, result.Name);
        }

        [Fact]
        public async Task GetAsync_WhenItemExpired_ReturnsNull()
        {
            // Arrange
            var cache = new MemoryAdvertisementCache(_logger.Object);
            var advertisement = CreateTestAdvertisement();
            await cache.SetAsync(advertisement.WebId, advertisement, TimeSpan.FromMilliseconds(1));

            // Wait for item to expire
            await Task.Delay(10);

            // Act
            var result = await cache.GetAsync(advertisement.WebId);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetAsync_WithInvalidId_ThrowsArgumentException(string id)
        {
            // Arrange
            var cache = new MemoryAdvertisementCache(_logger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => cache.GetAsync(id));
        }

        [Fact]
        public async Task SetAsync_WithNullAdvertisement_ThrowsArgumentNullException()
        {
            // Arrange
            var cache = new MemoryAdvertisementCache(_logger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => cache.SetAsync("test-id", null, TimeSpan.FromMinutes(1)));
        }

        [Fact]
        public async Task SetAsync_WithZeroDuration_ThrowsArgumentException()
        {
            // Arrange
            var cache = new MemoryAdvertisementCache(_logger.Object);
            var advertisement = CreateTestAdvertisement();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => cache.SetAsync(advertisement.WebId, advertisement, TimeSpan.Zero));
        }

        [Fact]
        public async Task SetAsync_WithNegativeDuration_ThrowsArgumentException()
        {
            // Arrange
            var cache = new MemoryAdvertisementCache(_logger.Object);
            var advertisement = CreateTestAdvertisement();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => cache.SetAsync(advertisement.WebId, advertisement, TimeSpan.FromMinutes(-1)));
        }
    }
}