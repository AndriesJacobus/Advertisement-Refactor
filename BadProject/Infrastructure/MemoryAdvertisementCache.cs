using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using BadProject.Configuration;
using BadProject.Core;
using ThirdParty;

namespace BadProject.Infrastructure
{
    public class MemoryAdvertisementCache : IAdvertisementCache
    {
        private readonly MemoryCache _cache;
        private readonly ILogger<MemoryAdvertisementCache> _logger;
        private readonly string _cacheKeyPrefix = "AdvKey_";
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public MemoryAdvertisementCache(
            ILogger<MemoryAdvertisementCache> logger)
        {
            _cache = new MemoryCache(typeof(MemoryAdvertisementCache).FullName);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Advertisement?> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID cannot be null or empty", nameof(id));

            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be whitespace", nameof(id));

            await _semaphore.WaitAsync();
            try
            {
                var key = GetCacheKey(id);
                var advertisement = _cache.Get(key) as Advertisement;

                _logger.LogDebug(
                    advertisement != null
                        ? "Cache hit for advertisement {Id}"
                        : "Cache miss for advertisement {Id}",
                    id);

                return advertisement;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SetAsync(string id, Advertisement advertisement, TimeSpan duration)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("ID cannot be null or empty", nameof(id));

            if (advertisement == null)
                throw new ArgumentNullException(nameof(advertisement));

            if (duration <= TimeSpan.Zero)
                throw new ArgumentException("Duration must be positive", nameof(duration));

            await _semaphore.WaitAsync();
            try
            {
                var key = GetCacheKey(id);
                _cache.Set(key, advertisement, DateTimeOffset.Now.Add(duration));

                _logger.LogDebug("Cached advertisement {Id} for {Duration:g}", id, duration);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private string GetCacheKey(string id) => $"{_cacheKeyPrefix}{id}";
    }
}