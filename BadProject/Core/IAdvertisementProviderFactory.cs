namespace BadProject.Core
{
    /// <summary>
    /// Factory interface for creating advertisement providers
    /// </summary>
    public interface IAdvertisementProviderFactory
    {
        /// <summary>
        /// Creates the primary (NoSQL) advertisement provider
        /// </summary>
        IAdvertisementProvider CreatePrimary();

        /// <summary>
        /// Creates the backup (SQL) advertisement provider
        /// </summary>
        IAdvertisementProvider CreateBackup();
    }
}