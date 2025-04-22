using System;
using System.Threading.Tasks;
using ThirdParty;

namespace BadProject.Core
{
    /// <summary>
    /// Defines the contract for advertisement caching
    /// </summary>
    public interface IAdvertisementCache
    {
        /// <summary>
        /// Gets an advertisement from cache
        /// </summary>
        /// <param name="id">The advertisement ID</param>
        /// <returns>The cached advertisement if found, null otherwise</returns>
        Task<Advertisement?> GetAsync(string id);

        /// <summary>
        /// Stores an advertisement in cache
        /// </summary>
        /// <param name="id">The advertisement ID</param>
        /// <param name="advertisement">The advertisement to cache</param>
        /// <param name="duration">How long to cache the advertisement</param>
        Task SetAsync(string id, Advertisement advertisement, TimeSpan duration);
    }
}