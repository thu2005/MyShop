using MyShop.Core.Models;

namespace MyShop.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for secure license data storage with dual-storage support.
    /// </summary>
    public interface ISecureStorageService
    {
        /// <summary>
        /// Saves the license info to both primary (registry) and backup (file) storage.
        /// </summary>
        /// <param name="licenseInfo">The license info to save.</param>
        void SaveLicenseInfo(LicenseInfo licenseInfo);

        /// <summary>
        /// Loads the license info from storage.
        /// Attempts primary storage first, falls back to backup if corrupted/missing.
        /// </summary>
        /// <returns>The license info, or null if not found.</returns>
        LicenseInfo? LoadLicenseInfo();

        /// <summary>
        /// Checks if any license data exists in storage.
        /// </summary>
        bool HasLicenseData();

        /// <summary>
        /// Clears all license data from both storage locations.
        /// </summary>
        void ClearLicenseData();
    }
}
