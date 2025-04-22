using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using BadProject.Configuration;
using BadProject.Core;
using ThirdParty;

namespace BadProject
{
    public class AdvertisementService
    {
        private readonly IAdvertisementProviderFactory _providerFactory;
        private readonly IAdvertisementCache _cache;
        private readonly ICircuitBreaker _circuitBreaker;
        private readonly ILogger<AdvertisementService> _logger;
        private readonly AdvertisementServiceOptions _options;

        public AdvertisementService(
            IAdvertisementProviderFactory providerFactory,
            IAdvertisementCache cache,
            ICircuitBreaker circuitBreaker,
            IOptions<AdvertisementServiceOptions> options,
            ILogger<AdvertisementService> logger)
        {
            _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
            if (options?.Value == null) throw new ArgumentNullException(nameof(options));
            _options = options.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Advertisement? GetAdvertisement(string id)
        {
            return GetAdvertisementAsync(id).GetAwaiter().GetResult();
        }

        private async Task<Advertisement?> GetAdvertisementAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Advertisement ID cannot be null or empty", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Advertisement ID cannot be whitespace", nameof(id));
            }

            try
            {
                // Try cache first
                var cached = await _cache.GetAsync(id);
                if (cached != null)
                {
                    _logger.LogDebug("Retrieved advertisement {Id} from cache", id);
                    return cached;
                }

                // Check if primary provider is available
                var usePrimary = await _circuitBreaker.IsAvailable();
                Advertisement? advertisement;

                if (usePrimary)
                {
                    try
                    {
                        var provider = _providerFactory.CreatePrimary();
                        advertisement = await provider.GetAsync(id);

                        if (advertisement != null)
                        {
                            await _circuitBreaker.RecordSuccess();
                            await CacheAdvertisement(id, advertisement);
                            return advertisement;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error fetching advertisement {Id} from primary provider", id);
                        await _circuitBreaker.RecordFailure();
                    }
                }

                // Try backup provider
                try
                {
                    _logger.LogInformation(
                        usePrimary
                            ? "Primary provider failed to retrieve advertisement {Id}, trying backup provider"
                            : "Circuit breaker is open, using backup provider for advertisement {Id}",
                        id);

                    var provider = _providerFactory.CreateBackup();
                    advertisement = await provider.GetAsync(id);

                    if (advertisement != null)
                    {
                        await CacheAdvertisement(id, advertisement);
                        return advertisement;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching advertisement {Id} from backup provider", id);
                    throw new AdvertisementNotFoundException(id, "Failed to retrieve advertisement from both providers", ex);
                }

                _logger.LogWarning("Advertisement {Id} not found in any provider", id);
                return null;
            }
            catch (Exception ex) when (ex is not AdvertisementNotFoundException)
            {
                _logger.LogError(ex, "Unexpected error retrieving advertisement {Id}", id);
                throw;
            }
        }

        private Task CacheAdvertisement(string id, Advertisement advertisement)
        {
            if (advertisement == null) return Task.CompletedTask;

            _logger.LogDebug("Caching advertisement {Id} for {Duration:g}", id, _options.CacheDuration);
            return _cache.SetAsync(id, advertisement, _options.CacheDuration);
        }
    }

    public class AdvertisementNotFoundException : Exception
    {
        public string AdvertisementId { get; }

        public AdvertisementNotFoundException(string id, string message, Exception innerException)
            : base(message, innerException)
        {
            AdvertisementId = id;
        }
    }
}
