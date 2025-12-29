using System;

namespace MyShop.Core.Models
{
    /// <summary>
    /// Represents the license/trial information for the application.
    /// </summary>
    public class LicenseInfo
    {
        /// <summary>
        /// Start date of the 15-day trial period.
        /// </summary>
        public DateTime TrialStartDate { get; set; }

        /// <summary>
        /// Last recorded run date - used to detect system clock rollback.
        /// </summary>
        public DateTime LastRunDate { get; set; }

        /// <summary>
        /// SHA256 hash of hardware fingerprint (CPU + Mainboard + Disk).
        /// Used for machine binding to prevent license transfer.
        /// </summary>
        public string MachineSignature { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the software has been activated with a full license.
        /// </summary>
        public bool IsActivated { get; set; }

        /// <summary>
        /// HMAC hash of the license data for integrity verification.
        /// </summary>
        public string DataHash { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the current license status.
    /// </summary>
    public enum LicenseStatus
    {
        /// <summary>Trial is active and valid.</summary>
        TrialActive,

        /// <summary>Trial has expired.</summary>
        TrialExpired,

        /// <summary>Full license is activated.</summary>
        Activated,

        /// <summary>License data is corrupted or tampered with.</summary>
        Invalid,

        /// <summary>Machine signature mismatch - license from different machine.</summary>
        MachineMismatch,

        /// <summary>System clock has been rolled back.</summary>
        ClockTampered
    }
}
