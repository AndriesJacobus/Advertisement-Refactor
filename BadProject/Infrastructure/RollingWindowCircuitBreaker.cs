using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using BadProject.Configuration;
using BadProject.Core;

namespace BadProject.Infrastructure
{
    public class RollingWindowCircuitBreaker : ICircuitBreaker
    {
        private readonly ConcurrentQueue<DateTime> _errors;
        private readonly ILogger<RollingWindowCircuitBreaker> _logger;
        private readonly AdvertisementServiceOptions _options;
        private readonly SemaphoreSlim _cleanupLock = new SemaphoreSlim(1, 1);

        public RollingWindowCircuitBreaker(
            IOptions<AdvertisementServiceOptions> options,
            ILogger<RollingWindowCircuitBreaker> logger)
        {
            if (options?.Value == null) throw new ArgumentNullException(nameof(options));
            _options = options.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _errors = new ConcurrentQueue<DateTime>();
        }

        public async Task<bool> IsAvailable()
        {
            await CleanupExpiredErrors();
            var errorCount = _errors.Count;
            var isAvailable = errorCount < _options.ErrorThreshold;

            _logger.LogDebug(
                isAvailable
                    ? "Circuit breaker is closed (service available). Current error count: {ErrorCount}"
                    : "Circuit breaker is open (service unavailable). Current error count: {ErrorCount}",
                errorCount);

            return isAvailable;
        }

        public async Task RecordSuccess()
        {
            await CleanupExpiredErrors();
            _logger.LogDebug("Successful operation recorded. Current error count: {ErrorCount}", _errors.Count);
        }

        public async Task RecordFailure()
        {
            await CleanupExpiredErrors();

            // Ensure we don't exceed the maximum error count
            while (_errors.Count >= _options.MaxErrorCount)
            {
                _errors.TryDequeue(out _);
            }

            _errors.Enqueue(DateTime.UtcNow);
            var errorCount = _errors.Count;

            _logger.LogWarning(
                "Failure recorded. Current error count: {ErrorCount}. Circuit breaker {Status}",
                errorCount,
                errorCount >= _options.ErrorThreshold ? "opened" : "remains closed");
        }

        public async Task<int> GetCurrentErrorCount()
        {
            await CleanupExpiredErrors();
            return _errors.Count;
        }

        private async Task CleanupExpiredErrors()
        {
            await _cleanupLock.WaitAsync();
            try
            {
                var threshold = DateTime.UtcNow.Subtract(_options.CircuitBreakerWindow);

                // Remove expired errors from the queue
                while (_errors.TryPeek(out DateTime oldestError) && oldestError < threshold)
                {
                    _errors.TryDequeue(out _);
                }
            }
            finally
            {
                _cleanupLock.Release();
            }
        }
    }
}