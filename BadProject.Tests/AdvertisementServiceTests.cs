using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BadProject.Core;
using BadProject.Configuration;
using BadProject.Tests.TestHelpers;
using ThirdParty;

namespace BadProject.Tests
{
    public class AdvertisementServiceTests : TestFixture
    {
        private readonly Mock<IAdvertisementProviderFactory> _providerFactory;
        private readonly Mock<IAdvertisementCache> _cache;
        private readonly Mock<ICircuitBreaker> _circuitBreaker;
        private readonly Mock<ILogger<AdvertisementService>> _logger;
        private readonly IOptions<AdvertisementServiceOptions> _options;
        private readonly Mock<IAdvertisementProvider> _primaryProvider;
        private readonly Mock<IAdvertisementProvider> _backupProvider;

        public AdvertisementServiceTests()
        {
            _providerFactory = new Mock<IAdvertisementProviderFactory>();
            _cache = new Mock<IAdvertisementCache>();
            _circuitBreaker = new Mock<ICircuitBreaker>();
            _logger = CreateLogger<AdvertisementService>();
            _options = CreateOptions();
            _primaryProvider = new Mock<IAdvertisementProvider>();
            _backupProvider = new Mock<IAdvertisementProvider>();

            _providerFactory.Setup(x => x.CreatePrimary()).Returns(_primaryProvider.Object);
            _providerFactory.Setup(x => x.CreateBackup()).Returns(_backupProvider.Object);
        }

        [Fact]
        public void GetAdvertisement_WithInvalidId_ThrowsArgumentException()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => service.GetAdvertisement(null));
            Assert.Throws<ArgumentException>(() => service.GetAdvertisement(string.Empty));
            Assert.Throws<ArgumentException>(() => service.GetAdvertisement(" "));
        }

        [Fact]
        public void GetAdvertisement_WhenInCache_ReturnsCachedItem()
        {
            // Arrange
            var service = CreateService();
            var expected = CreateTestAdvertisement();
            _cache.Setup(x => x.GetAsync(expected.WebId))
                .ReturnsAsync(expected);

            // Act
            var result = service.GetAdvertisement(expected.WebId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.WebId, result.WebId);
            _primaryProvider.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Never);
            _backupProvider.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetAdvertisement_WhenNotInCacheAndCircuitClosed_UsesPrimaryProvider()
        {
            // Arrange
            var service = CreateService();
            var expected = CreateTestAdvertisement();
            _cache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync((Advertisement)null);
            _circuitBreaker.Setup(x => x.IsAvailable()).ReturnsAsync(true);
            _primaryProvider.Setup(x => x.GetAsync(expected.WebId)).ReturnsAsync(expected);

            // Act
            var result = service.GetAdvertisement(expected.WebId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.WebId, result.WebId);
            _primaryProvider.Verify(x => x.GetAsync(expected.WebId), Times.Once);
            _backupProvider.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetAdvertisement_WhenPrimaryFailsAndCircuitOpen_UsesBackupProvider()
        {
            // Arrange
            var service = CreateService();
            var expected = CreateTestAdvertisement();
            _cache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync((Advertisement)null);
            _circuitBreaker.Setup(x => x.IsAvailable()).ReturnsAsync(false);
            _backupProvider.Setup(x => x.GetAsync(expected.WebId)).ReturnsAsync(expected);

            // Act
            var result = service.GetAdvertisement(expected.WebId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.WebId, result.WebId);
            _primaryProvider.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Never);
            _backupProvider.Verify(x => x.GetAsync(expected.WebId), Times.Once);
        }

        [Fact]
        public void GetAdvertisement_WhenBothProvidersFail_ThrowsException()
        {
            // Arrange
            var service = CreateService();
            _cache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync((Advertisement)null);
            _circuitBreaker.Setup(x => x.IsAvailable()).ReturnsAsync(true);
            _primaryProvider.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Primary failed"));
            _backupProvider.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Backup failed"));

            // Act & Assert
            var ex = Assert.Throws<AdvertisementNotFoundException>(
                () => service.GetAdvertisement("test-id"));
            Assert.Contains("both providers", ex.Message);
        }

        [Fact]
        public void GetAdvertisement_WhenSuccessful_CachesResult()
        {
            // Arrange
            var service = CreateService();
            var expected = CreateTestAdvertisement();
            _cache.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync((Advertisement)null);
            _circuitBreaker.Setup(x => x.IsAvailable()).ReturnsAsync(true);
            _primaryProvider.Setup(x => x.GetAsync(expected.WebId)).ReturnsAsync(expected);

            // Act
            var result = service.GetAdvertisement(expected.WebId);

            // Assert
            Assert.NotNull(result);
            _cache.Verify(x => x.SetAsync(
                expected.WebId,
                expected,
                It.Is<TimeSpan>(ts => ts == _options.Value.CacheDuration)),
                Times.Once);
        }

        private AdvertisementService CreateService()
        {
            return new AdvertisementService(
                _providerFactory.Object,
                _cache.Object,
                _circuitBreaker.Object,
                _options,
                _logger.Object);
        }
    }
}