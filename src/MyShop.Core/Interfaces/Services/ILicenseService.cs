using MyShop.Core.Models;

namespace MyShop.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for managing application licensing and trial status.
    /// </summary>
    public interface ILicenseService
    {
        /// <summary>
        /// Gets the current license status.
        /// </summary>
        LicenseStatus GetLicenseStatus();

        /// <summary>
        /// Gets the number of days remaining in the trial period.
        /// Returns 0 if trial expired, -1 if fully activated.
        /// </summary>
        int GetRemainingTrialDays();

        /// <summary>
        /// Checks if a specific feature is allowed based on current license status.
        /// </summary>
        /// <param name="featureName">The name of the feature to check.</param>
        /// <returns>True if the feature is allowed, false otherwise.</returns>
        bool IsFeatureAllowed(string featureName);

        /// <summary>
        /// Initializes a new trial if none exists.
        /// Called on first run of the application.
        /// </summary>
        void InitializeTrial();

        /// <summary>
        /// Records the current app run timestamp.
        /// Used for clock rollback detection.
        /// </summary>
        void RecordAppRun();

        /// <summary>
        /// Attempts to activate a full license using a license key.
        /// </summary>
        /// <param name="licenseKey">The license key to validate.</param>
        /// <returns>True if activation successful, false otherwise.</returns>
        bool ActivateLicense(string licenseKey);

        /// <summary>
        /// Gets a user-friendly message describing the current license status.
        /// </summary>
        string GetStatusMessage();

#if DEBUG
        /// <summary>
        /// DEBUG ONLY: Forces trial expiration.
        /// </summary>
        void ForceTrialExpired();

        /// <summary>
        /// DEBUG ONLY: Resets trial license data.
        /// </summary>
        void ResetTrial();
#endif
    }
}
