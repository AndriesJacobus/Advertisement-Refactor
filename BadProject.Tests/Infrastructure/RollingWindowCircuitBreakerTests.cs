using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using BadProject.Configuration;
using BadProject.Infrastructure;
using BadProject.Tests.TestHelpers;

namespace BadProject.Tests.Infrastructure
{
    public class RollingWindowCircuitBreakerTests : TestFixture
    {
        private readonly Mock<ILogger<RollingWindowCircuitBreaker>> _logger;
        private readonly TimeSpan _window = TimeSpan.FromMinutes(1);
        private readonly int _threshold = 3;

        public RollingWindowCircuitBreakerTests()
        {
            _logger = CreateLogger<RollingWindowCircuitBreaker>();
        }

        private IOptions<AdvertisementServiceOptions> CreateCircuitBreakerOptions()
        {
            return CreateOptions(opt =>
            {
                opt.CircuitBreakerWindow = _window;
                opt.ErrorThreshold = _threshold;
                opt.MaxErrorCount = _threshold * 2;
            });
        }

        [Fact]
        public async Task IsAvailable_WhenNoErrors_ReturnsTrue()
        {
            // Arrange
            var breaker = new RollingWindowCircuitBreaker(CreateCircuitBreakerOptions(), _logger.Object);

            // Act
            var result = await breaker.IsAvailable();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsAvailable_WhenErrorsExceedThreshold_ReturnsFalse()
        {
            // Arrange
            var breaker = new RollingWindowCircuitBreaker(CreateCircuitBreakerOptions(), _logger.Object);

            // Act
            for (int i = 0; i <= _threshold; i++)
            {
                await breaker.RecordFailure();
            }
            var result = await breaker.IsAvailable();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsAvailable_WhenErrorsExpire_ReturnsTrue()
        {
            // Arrange
            var options = CreateOptions(opt =>
            {
                opt.CircuitBreakerWindow = TimeSpan.FromMilliseconds(50);
                opt.ErrorThreshold = _threshold;
                opt.MaxErrorCount = _threshold * 2;
            });
            var breaker = new RollingWindowCircuitBreaker(options, _logger.Object);

            // Act
            for (int i = 0; i <= _threshold; i++)
            {
                await breaker.RecordFailure();
            }

            // Wait for errors to expire
            await Task.Delay(100);
            var result = await breaker.IsAvailable();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetCurrentErrorCount_ReturnsCorrectCount()
        {
            // Arrange
            var breaker = new RollingWindowCircuitBreaker(CreateCircuitBreakerOptions(), _logger.Object);
            var expectedErrors = 2;

            // Act
            for (int i = 0; i < expectedErrors; i++)
            {
                await breaker.RecordFailure();
            }
            var count = await breaker.GetCurrentErrorCount();

            // Assert
            Assert.Equal(expectedErrors, count);
        }

        [Fact]
        public async Task RecordSuccess_DoesNotAffectErrorCount()
        {
            // Arrange
            var breaker = new RollingWindowCircuitBreaker(CreateCircuitBreakerOptions(), _logger.Object);
            await breaker.RecordFailure();
            var beforeCount = await breaker.GetCurrentErrorCount();

            // Act
            await breaker.RecordSuccess();
            var afterCount = await breaker.GetCurrentErrorCount();

            // Assert
            Assert.Equal(beforeCount, afterCount);
        }
    }
}