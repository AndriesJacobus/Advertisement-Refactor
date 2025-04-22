using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using BadProject.Configuration;
using BadProject.Core;
using ThirdParty;

namespace BadProject.Providers
{
    public class SqlAdvertisementAdapter : IAdvertisementProvider
    {
        private readonly ILogger<SqlAdvertisementAdapter> _logger;
        private readonly ProviderRetryOptions _retryOptions;

        public SqlAdvertisementAdapter(
            IOptions<AdvertisementServiceOptions> options,
            ILogger<SqlAdvertisementAdapter> logger)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));
            _retryOptions = options.Value?.SqlProviderOptions ?? throw new ArgumentNullException(nameof(options) + "." + nameof(AdvertisementServiceOptions.SqlProviderOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Advertisement?> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Advertisement ID cannot be null or empty", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Advertisement ID cannot be whitespace", nameof(id));
            }

            int retryCount = 0;
            Exception lastException = null;

            do
            {
                try
                {
                    _logger.LogInformation(
                        "Attempting to fetch advertisement {Id} from SQL provider. Attempt {Attempt}/{MaxAttempts}",
                        id, retryCount + 1, _retryOptions.RetryCount + 1);

                    Advertisement? advertisement = SQLAdvProvider.GetAdv(id);

                    if (advertisement != null)
                    {
                        _logger.LogInformation("Successfully retrieved advertisement {Id} from SQL provider", id);
                        return advertisement;
                    }

                    _logger.LogWarning("No advertisement found with ID {Id} in SQL provider", id);
                    return null;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    retryCount++;

                    if (retryCount <= _retryOptions.RetryCount)
                    {
                        // Calculate delay with exponential backoff
                        var delay = TimeSpan.FromTicks(_retryOptions.RetryDelay.Ticks * (long)Math.Pow(2, retryCount - 1));
                        delay = delay > _retryOptions.MaxRetryDelay ? _retryOptions.MaxRetryDelay : delay;

                        _logger.LogWarning(ex,
                            "Error fetching advertisement {Id} from SQL provider. Attempt {Attempt}/{MaxAttempts}. Retrying in {Delay:N3}s",
                            id, retryCount, _retryOptions.RetryCount + 1, delay.TotalSeconds);

                        await Task.Delay(delay);
                    }
                }
            } while (retryCount <= _retryOptions.RetryCount);

            _logger.LogError(lastException,
                "Failed to fetch advertisement {Id} from SQL provider after {Attempts} attempts",
                id, retryCount);

            throw new AdvertisementProviderException(
                $"Failed to fetch advertisement {id} from SQL provider after {retryCount} attempts",
                lastException);
        }
    }
}