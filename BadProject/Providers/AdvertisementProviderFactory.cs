using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using BadProject.Configuration;
using BadProject.Core;
using ThirdParty;

namespace BadProject.Providers
{
    public class AdvertisementProviderFactory : IAdvertisementProviderFactory
    {
        private readonly IOptions<AdvertisementServiceOptions> _options;
        private readonly ILoggerFactory _loggerFactory;

        public AdvertisementProviderFactory(
            IOptions<AdvertisementServiceOptions> options,
            ILoggerFactory loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IAdvertisementProvider CreatePrimary()
        {
            return new NoSqlAdvertisementAdapter(
                new NoSqlAdvProvider(),
                _options,
                _loggerFactory.CreateLogger<NoSqlAdvertisementAdapter>());
        }

        public IAdvertisementProvider CreateBackup()
        {
            return new SqlAdvertisementAdapter(
                _options,
                _loggerFactory.CreateLogger<SqlAdvertisementAdapter>());
        }
    }
}