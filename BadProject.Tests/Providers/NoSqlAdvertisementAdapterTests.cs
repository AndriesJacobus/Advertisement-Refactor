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
    public class NoSqlAdvertisementAdapterTests : TestFixture
    {
        private readonly Mock<ILogger<NoSqlAdvertisementAdapter>> _logger;
        private readonly IOptions<AdvertisementServiceOptions> _options;
        private readonly NoSqlAdvProvider _provider;

        public NoSqlAdvertisementAdapterTests()
        {
            _logger = CreateLogger<NoSqlAdvertisementAdapter>();
            _options = CreateOptions();
            _provider = new NoSqlAdvProvider();
        }

        [Fact]
        public async Task GetAsync_WithValidId_ReturnsAdvertisement()
        {
            // Arrange
            var adapter = new NoSqlAdvertisementAdapter(_provider, _options, _logger.Object);
            var id = "test-id";

            // Act
            var result = await adapter.GetAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result?.WebId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetAsync_WithInvalidId_ThrowsArgumentException(string id)
        {
            // Arrange
            var adapter = new NoSqlAdvertisementAdapter(_provider, _options, _logger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => adapter.GetAsync(id));
        }
    }
}