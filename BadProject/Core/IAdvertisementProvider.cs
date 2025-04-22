using System.Threading.Tasks;
using ThirdParty;

namespace BadProject.Core
{
    /// <summary>
    /// Defines the contract for advertisement providers
    /// </summary>
    public interface IAdvertisementProvider
    {
        /// <summary>
        /// Retrieves an advertisement by its ID
        /// </summary>
        /// <param name="id">The unique identifier for the advertisement</param>
        /// <returns>The advertisement if found, null otherwise</returns>
        Task<Advertisement?> GetAsync(string id);
    }
}