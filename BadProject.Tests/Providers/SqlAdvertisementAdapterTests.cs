using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BadProject.Configuration;
using BadProject.Providers;
using BadProject.Tests.TestHelpers;
using ThirdParty;

namespace BadProject.Tests.Providers
{
    public class SqlAdvertisementAdapterTests : TestFixture
    {
        private readonly Mock<ILogger<SqlAdvertisementAdapter>> _logger;
        private readonly IOptions<AdvertisementServiceOptions> _options;

        public SqlAdvertisementAdapterTests()
        {
            _logger = CreateLogger<SqlAdvertisementAdapter>();
            _options = CreateOptions();
        }

        [Fact]
        public async Task GetAsync_WithValidId_ReturnsAdvertisement()
        {
            // Arrange
            var adapter = new SqlAdvertisementAdapter(_options, _logger.Object);
            var id = "test-id";

            // Act
            var result = await adapter.GetAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.WebId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetAsync_WithInvalidId_ThrowsArgumentException(string id)
        {
            // Arrange
            var adapter = new SqlAdvertisementAdapter(_options, _logger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => adapter.GetAsync(id));
        }

        [Fact]
        public async Task GetAsync_LogsSuccessfulRetrieval()
        {
            // Arrange
            var adapter = new SqlAdvertisementAdapter(_options, _logger.Object);

            // Act
            var result = await adapter.GetAsync("test-id");

            // Assert
            _logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Successfully retrieved advertisement")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}