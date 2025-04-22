using System;
using System.Configuration;
using Xunit;
using BadProject.Configuration;

namespace BadProject.Tests.Configuration
{
    [Collection("NonParallelTests")]
    public class ConfigurationLoaderTests : IDisposable
    {
        public ConfigurationLoaderTests()
        {
            // Setup - Reset config to safe defaults before each test
            ResetConfigToDefaults();
        }

        public void Dispose()
        {
            // Cleanup after each test if needed (not critical here)
        }

        [Fact]
        public void LoadFromConfig_WithDefaultSettings_LoadsDefaultValues()
        {
            // Act
            var options = ConfigurationLoader.LoadFromConfig();

            // Assert
            Assert.Equal(3, options.NoSqlProviderOptions.RetryCount);
            Assert.Equal(TimeSpan.FromSeconds(1), options.NoSqlProviderOptions.RetryDelay);
            Assert.Equal(TimeSpan.FromSeconds(30), options.NoSqlProviderOptions.MaxRetryDelay);

            Assert.Equal(2, options.SqlProviderOptions.RetryCount);
            Assert.Equal(TimeSpan.FromSeconds(2), options.SqlProviderOptions.RetryDelay);
            Assert.Equal(TimeSpan.FromSeconds(30), options.SqlProviderOptions.MaxRetryDelay);

            Assert.Equal(TimeSpan.FromMinutes(5), options.CacheDuration);
            Assert.Equal(TimeSpan.FromHours(1), options.CircuitBreakerWindow);
            Assert.Equal(10, options.ErrorThreshold);
            Assert.Equal(20, options.MaxErrorCount);
        }

        [Fact]
        public void LoadFromConfig_WithCustomSettings_LoadsCustomValues()
        {
            // Arrange
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = config.AppSettings.Settings;

            SetOrAddSetting(settings, "NoSql.RetryCount", "5");
            SetOrAddSetting(settings, "NoSql.RetryDelaySeconds", "2");
            SetOrAddSetting(settings, "NoSql.MaxRetryDelaySeconds", "60");

            SetOrAddSetting(settings, "Sql.RetryCount", "3");
            SetOrAddSetting(settings, "Sql.RetryDelaySeconds", "3");
            SetOrAddSetting(settings, "Sql.MaxRetryDelaySeconds", "45");

            SetOrAddSetting(settings, "Cache.DurationMinutes", "10");
            SetOrAddSetting(settings, "CircuitBreaker.WindowHours", "2");
            SetOrAddSetting(settings, "CircuitBreaker.ErrorThreshold", "15");
            SetOrAddSetting(settings, "CircuitBreaker.MaxErrorCount", "25");

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            // Act
            var options = ConfigurationLoader.LoadFromConfig();

            // Assert
            Assert.Equal(5, options.NoSqlProviderOptions.RetryCount);
            Assert.Equal(TimeSpan.FromSeconds(2), options.NoSqlProviderOptions.RetryDelay);
            Assert.Equal(TimeSpan.FromSeconds(60), options.NoSqlProviderOptions.MaxRetryDelay);

            Assert.Equal(3, options.SqlProviderOptions.RetryCount);
            Assert.Equal(TimeSpan.FromSeconds(3), options.SqlProviderOptions.RetryDelay);
            Assert.Equal(TimeSpan.FromSeconds(45), options.SqlProviderOptions.MaxRetryDelay);

            Assert.Equal(TimeSpan.FromMinutes(10), options.CacheDuration);
            Assert.Equal(TimeSpan.FromHours(2), options.CircuitBreakerWindow);
            Assert.Equal(15, options.ErrorThreshold);
            Assert.Equal(25, options.MaxErrorCount);
        }

        [Fact]
        public void LoadFromConfig_WithInvalidSettings_UsesDefaultValues()
        {
            // Arrange
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = config.AppSettings.Settings;

            SetOrAddSetting(settings, "NoSql.RetryCount", "invalid");
            SetOrAddSetting(settings, "NoSql.RetryDelaySeconds", "invalid");
            SetOrAddSetting(settings, "Cache.DurationMinutes", "invalid");

            // Critical: Keep valid ErrorThreshold to avoid crash
            SetOrAddSetting(settings, "CircuitBreaker.ErrorThreshold", "10");
            SetOrAddSetting(settings, "CircuitBreaker.MaxErrorCount", "20");

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            // Act
            var options = ConfigurationLoader.LoadFromConfig();

            // Assert
            Assert.Equal(3, options.NoSqlProviderOptions.RetryCount); // Default
            Assert.Equal(TimeSpan.FromSeconds(1), options.NoSqlProviderOptions.RetryDelay); // Default
            Assert.Equal(TimeSpan.FromMinutes(5), options.CacheDuration); // Default
        }

        [Fact]
        public void LoadFromConfig_WithInvalidConfiguration_ThrowsOnValidation()
        {
            // Arrange
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = config.AppSettings.Settings;

            SetOrAddSetting(settings, "CircuitBreaker.ErrorThreshold", "0"); // Invalid: must be >= 1
            SetOrAddSetting(settings, "CircuitBreaker.MaxErrorCount", "5"); // Invalid: must be >= ErrorThreshold

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ConfigurationLoader.LoadFromConfig());
        }

        [Fact]
        public void LoadFromConfig_WithNegativeTimeSpans_ThrowsOnValidation()
        {
            // Arrange
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = config.AppSettings.Settings;

            SetOrAddSetting(settings, "NoSql.RetryDelaySeconds", "-1"); // Negative RetryDelay

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ConfigurationLoader.LoadFromConfig());
        }

        private void SetOrAddSetting(KeyValueConfigurationCollection settings, string key, string value)
        {
            if (settings[key] == null)
                settings.Add(key, value);
            else
                settings[key].Value = value;
        }

        private void ResetConfigToDefaults()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = config.AppSettings.Settings;

            // Set all defaults
            SetOrAddSetting(settings, "NoSql.RetryCount", "3");
            SetOrAddSetting(settings, "NoSql.RetryDelaySeconds", "1");
            SetOrAddSetting(settings, "NoSql.MaxRetryDelaySeconds", "30");

            SetOrAddSetting(settings, "Sql.RetryCount", "2");
            SetOrAddSetting(settings, "Sql.RetryDelaySeconds", "2");
            SetOrAddSetting(settings, "Sql.MaxRetryDelaySeconds", "30");

            SetOrAddSetting(settings, "Cache.DurationMinutes", "5");
            SetOrAddSetting(settings, "CircuitBreaker.WindowHours", "1");
            SetOrAddSetting(settings, "CircuitBreaker.ErrorThreshold", "10");
            SetOrAddSetting(settings, "CircuitBreaker.MaxErrorCount", "20");

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }

    [CollectionDefinition("NonParallelTests", DisableParallelization = true)]
    public class NonParallelTestsCollection { }
}
