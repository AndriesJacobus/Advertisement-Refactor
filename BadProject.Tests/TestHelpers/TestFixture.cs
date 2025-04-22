using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using BadProject.Configuration;
using ThirdParty;

namespace BadProject.Tests.TestHelpers
{
    public class TestFixture
    {
        protected Mock<ILogger<T>> CreateLogger<T>()
        {
            return new Mock<ILogger<T>>();
        }

        protected IOptions<AdvertisementServiceOptions> CreateOptions(Action<AdvertisementServiceOptions>? configure = null)
        {
            var options = new AdvertisementServiceOptions();
            configure?.Invoke(options);
            return Options.Create(options);
        }

        protected static Advertisement CreateTestAdvertisement(string id = "test-id")
        {
            return new Advertisement
            {
                WebId = id,
                Name = $"Test Ad {id}",
                Description = "Test Description"
            };
        }
    }
}