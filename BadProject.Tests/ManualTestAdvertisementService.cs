using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BadProject.Configuration;
using BadProject.Core;
using BadProject.Infrastructure;
using BadProject.Providers;
using BadProject.Tests.TestHelpers;
using ThirdParty;

namespace BadProject.Tests
{
    public class ManualTestAdvertisementService : TestFixture
    {
        [Fact(Skip = "Manual test only - not for automated testing")]
        public void RunAdvertisementService()
        {
            // Set up logger factory
            var loggerFactory = LoggingConfiguration.CreateLoggerFactory();

            // Load configuration
            var options = ConfigurationLoader.LoadFromConfig();
            var optionsWrapper = Options.Create(options);

            // Create dependencies
            var cache = new MemoryAdvertisementCache(
                loggerFactory.CreateLogger<MemoryAdvertisementCache>());

            var circuitBreaker = new RollingWindowCircuitBreaker(
                optionsWrapper,
                loggerFactory.CreateLogger<RollingWindowCircuitBreaker>());

            var providerFactory = new AdvertisementProviderFactory(
                optionsWrapper,
                loggerFactory);

            // Create the service
            var advertisementService = new AdvertisementService(
                providerFactory,
                cache,
                circuitBreaker,
                optionsWrapper,
                loggerFactory.CreateLogger<AdvertisementService>());

            // Define test IDs
            string[] testIds = new string[] { "test-id", "popular-ad", "nonexistent-id", "promo-special" };

            Console.WriteLine("======= Advertisement Service Test =======");
            Console.WriteLine("Configuration Options:");
            Console.WriteLine($"- NoSQL Provider Retry Count: {options.NoSqlProviderOptions.RetryCount}");
            Console.WriteLine($"- SQL Provider Retry Count: {options.SqlProviderOptions.RetryCount}");
            Console.WriteLine($"- Cache Duration: {options.CacheDuration}");
            Console.WriteLine($"- Error Threshold: {options.ErrorThreshold}");
            Console.WriteLine();

            // Try to retrieve advertisements
            foreach (var id in testIds)
            {
                try
                {
                    Console.WriteLine($"Attempting to retrieve advertisement: {id}");
                    var advertisement = advertisementService.GetAdvertisement(id);

                    if (advertisement != null)
                    {
                        Console.WriteLine($"SUCCESS: Found advertisement");
                        Console.WriteLine($"- Web ID: {advertisement.WebId}");
                        Console.WriteLine($"- Campaign Name: {advertisement.Name}");
                        Console.WriteLine($"- Campaign Description: {advertisement.Description}");
                    }
                    else
                    {
                        Console.WriteLine($"NOT FOUND: No advertisement exists with ID '{id}'");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.GetType().Name} - {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"  Inner Exception: {ex.InnerException.Message}");
                    }
                }

                Console.WriteLine(new string('-', 50));
            }

            // Try the cache (second request to first ID should be cached)
            string cachedId = testIds[0];
            Console.WriteLine($"Testing cache with ID: {cachedId} (should be cached now)");
            try
            {
                var advertisement = advertisementService.GetAdvertisement(cachedId);
                Console.WriteLine($"Result: {(advertisement != null ? "Found in cache" : "Not found")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }

            Console.WriteLine("======= Test Complete =======");

            // No asserts in manual test - just for observation
            // You can set a breakpoint here to examine results before the test ends
            System.Diagnostics.Debugger.Break();
        }

        [Fact(Skip = "Circuit breaker test - not for automated testing")]
        public void TestCircuitBreaker()
        {
            // Set up logger factory
            var loggerFactory = LoggingConfiguration.CreateLoggerFactory();

            // Load configuration with low error threshold for faster testing
            var options = ConfigurationLoader.LoadFromConfig();
            options.ErrorThreshold = 2; // Set low threshold for quicker testing
            var optionsWrapper = Options.Create(options);

            // Create dependencies
            var cache = new MemoryAdvertisementCache(
                loggerFactory.CreateLogger<MemoryAdvertisementCache>());

            var circuitBreaker = new RollingWindowCircuitBreaker(
                optionsWrapper,
                loggerFactory.CreateLogger<RollingWindowCircuitBreaker>());

            // Create mock provider factory that throws exceptions for primary
            var providerFactory = new AdvertisementProviderFactory(
                optionsWrapper,
                loggerFactory);

            // Create the service
            var advertisementService = new AdvertisementService(
                providerFactory,
                cache,
                circuitBreaker,
                optionsWrapper,
                loggerFactory.CreateLogger<AdvertisementService>());

            Console.WriteLine("=== Circuit Breaker Test ===");
            Console.WriteLine($"Error Threshold: {options.ErrorThreshold}");
            Console.WriteLine($"Window Duration: {options.CircuitBreakerWindow}");

            // Force errors to trigger circuit breaker
            string badId = "trigger-error";  // An ID that will cause errors

            Console.WriteLine("\nCausing errors to trigger circuit breaker...");
            for (int i = 0; i < options.ErrorThreshold + 2; i++)
            {
                try
                {
                    Console.WriteLine($"Attempt {i + 1}...");
                    var advertisement = advertisementService.GetAdvertisement(badId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Expected error: {ex.Message}");
                }
            }

            // Check circuit breaker status
            Console.WriteLine("\nChecking if circuit breaker is open...");
            var isAvailable = circuitBreaker.IsAvailable().GetAwaiter().GetResult();
            Console.WriteLine($"Circuit is {(isAvailable ? "CLOSED (available)" : "OPEN (unavailable)")}");
            var errorCount = circuitBreaker.GetCurrentErrorCount().GetAwaiter().GetResult();
            Console.WriteLine($"Current error count: {errorCount}");

            // Try a good request to see if it uses backup
            Console.WriteLine("\nTrying a good request with circuit open...");
            try
            {
                var advertisement = advertisementService.GetAdvertisement("test-id");
                Console.WriteLine($"Result: {(advertisement != null ? "Success with backup" : "Not found")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }

            Console.WriteLine("=== Circuit Breaker Test Complete ===");

            System.Diagnostics.Debugger.Break();
        }
    }
}