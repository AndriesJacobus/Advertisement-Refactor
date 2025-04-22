using System.Threading.Tasks;

namespace BadProject.Core
{
    /// <summary>
    /// Defines the contract for circuit breaker pattern implementation
    /// </summary>
    public interface ICircuitBreaker
    {
        /// <summary>
        /// Checks if the circuit is closed and the service is available
        /// </summary>
        Task<bool> IsAvailable();

        /// <summary>
        /// Records a successful operation
        /// </summary>
        Task RecordSuccess();

        /// <summary>
        /// Records a failed operation
        /// </summary>
        Task RecordFailure();

        /// <summary>
        /// Gets the current error count in the tracking window
        /// </summary>
        Task<int> GetCurrentErrorCount();
    }
}