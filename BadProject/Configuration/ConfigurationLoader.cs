using System;
using System.Configuration;

namespace BadProject.Configuration
{
    public static class ConfigurationLoader
    {
        public static AdvertisementServiceOptions LoadFromConfig()
        {
            var options = new AdvertisementServiceOptions
            {
                NoSqlProviderOptions = new ProviderRetryOptions
                {
                    RetryCount = GetConfigInt("NoSql.RetryCount", 3),
                    RetryDelay = TimeSpan.FromSeconds(GetConfigInt("NoSql.RetryDelaySeconds", 1)),
                    MaxRetryDelay = TimeSpan.FromSeconds(GetConfigInt("NoSql.MaxRetryDelaySeconds", 30))
                },

                SqlProviderOptions = new ProviderRetryOptions
                {
                    RetryCount = GetConfigInt("Sql.RetryCount", 2),
                    RetryDelay = TimeSpan.FromSeconds(GetConfigInt("Sql.RetryDelaySeconds", 2)),
                    MaxRetryDelay = TimeSpan.FromSeconds(GetConfigInt("Sql.MaxRetryDelaySeconds", 30))
                },

                CacheDuration = TimeSpan.FromMinutes(GetConfigInt("Cache.DurationMinutes", 5)),
                CircuitBreakerWindow = TimeSpan.FromHours(GetConfigInt("CircuitBreaker.WindowHours", 1)),
                ErrorThreshold = GetConfigInt("CircuitBreaker.ErrorThreshold", 10),
                MaxErrorCount = GetConfigInt("CircuitBreaker.MaxErrorCount", 20)
            };

            options.Validate();
            return options;
        }

        private static int GetConfigInt(string key, int defaultValue)
        {
            string? value = ConfigurationManager.AppSettings[key];
            if (value != null && int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}