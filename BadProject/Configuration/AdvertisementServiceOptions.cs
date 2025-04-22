using System;

namespace BadProject.Configuration
{
    public class ProviderRetryOptions
    {
        /// <summary>
        /// Number of retry attempts for the provider
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Base delay between retries (will be used with exponential backoff)
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Maximum delay between retries
        /// </summary>
        public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
    }

    public class AdvertisementServiceOptions
    {
        /// <summary>
        /// NoSQL provider retry configuration
        /// </summary>
        public ProviderRetryOptions NoSqlProviderOptions { get; set; } = new ProviderRetryOptions();

        /// <summary>
        /// SQL provider retry configuration
        /// </summary>
        public ProviderRetryOptions SqlProviderOptions { get; set; } = new ProviderRetryOptions
        {
            RetryCount = 2,  // Different default for SQL as it's the backup provider
            RetryDelay = TimeSpan.FromSeconds(2)
        };

        /// <summary>
        /// Duration to cache advertisements
        /// </summary>
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Time window for tracking errors
        /// </summary>
        public TimeSpan CircuitBreakerWindow { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// Number of errors before triggering failover
        /// </summary>
        public int ErrorThreshold { get; set; } = 10;

        /// <summary>
        /// Maximum number of errors to track
        /// </summary>
        public int MaxErrorCount { get; set; } = 20;

        /// <summary>
        /// Validates the configuration options
        /// </summary>
        public void Validate()
        {
            ValidateRetryOptions(NoSqlProviderOptions, nameof(NoSqlProviderOptions));
            ValidateRetryOptions(SqlProviderOptions, nameof(SqlProviderOptions));

            if (CacheDuration <= TimeSpan.Zero)
                throw new ArgumentException("CacheDuration must be positive", nameof(CacheDuration));

            if (CircuitBreakerWindow <= TimeSpan.Zero)
                throw new ArgumentException("CircuitBreakerWindow must be positive", nameof(CircuitBreakerWindow));

            if (ErrorThreshold < 1)
                throw new ArgumentException("ErrorThreshold must be at least 1", nameof(ErrorThreshold));

            if (MaxErrorCount < ErrorThreshold)
                throw new ArgumentException("MaxErrorCount must be greater than or equal to ErrorThreshold", nameof(MaxErrorCount));
        }

        private void ValidateRetryOptions(ProviderRetryOptions options, string paramName)
        {
            if (options == null)
                throw new ArgumentNullException(paramName);

            if (options.RetryCount < 0)
                throw new ArgumentException($"{paramName}: RetryCount must be non-negative");

            if (options.RetryDelay <= TimeSpan.Zero)
                throw new ArgumentException($"{paramName}: RetryDelay must be positive");

            if (options.MaxRetryDelay < options.RetryDelay)
                throw new ArgumentException($"{paramName}: MaxRetryDelay must be greater than or equal to RetryDelay");
        }
    }
}