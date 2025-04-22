using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BadProject.Configuration;
using BadProject.Core;
using BadProject.Infrastructure;
using BadProject.Providers;

namespace BadProject
{
    public class Program
    {
        public static void Main(string[] args)
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

            Console.WriteLine("======= Advertisement Service Test =======");

            while (true)
            {
                Console.WriteLine("\nEnter advertisement ID to retrieve (or 'exit' to quit):");
                string id = Console.ReadLine();

                if (id?.ToLower() == "exit")
                    break;

                try
                {
                    var advertisement = advertisementService.GetAdvertisement(id);

                    if (advertisement != null)
                    {
                        Console.WriteLine($"SUCCESS: Found advertisement '{advertisement.Name}'");
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
            }
        }
    }
}