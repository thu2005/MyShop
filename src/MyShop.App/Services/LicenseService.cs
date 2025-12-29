using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShop.App.Services
{
    /// <summary>
    /// Manages application licensing and trial status.
    /// </summary>
    public class LicenseService : ILicenseService
    {
        private readonly IFingerprintService _fingerprintService;
        private readonly ISecureStorageService _storageService;
        private LicenseInfo? _cachedLicenseInfo;
        private bool _isNewlyCreated; // Skip clock check for freshly created licenses

        private const int TrialDays = 15;

        // Features that require active trial or full license
        private static readonly HashSet<string> RestrictedFeatures = new(StringComparer.OrdinalIgnoreCase)
        {
            "CreateOrder",
            "AddProduct",
            "EditProduct",
            "DeleteProduct",
            "AddCustomer",
            "EditCustomer"
        };

        public LicenseService(
            IFingerprintService fingerprintService,
            ISecureStorageService storageService)
        {
            _fingerprintService = fingerprintService;
            _storageService = storageService;

            // Load cached license info
            _cachedLicenseInfo = _storageService.LoadLicenseInfo();
        }

        /// <inheritdoc />
        public LicenseStatus GetLicenseStatus()
        {
            if (_cachedLicenseInfo == null)
            {
                return LicenseStatus.Invalid;
            }

            // Check if fully activated
            if (_cachedLicenseInfo.IsActivated)
            {
                // Still verify machine binding
                var currentSignature = _fingerprintService.GetMachineSignature();
                if (_cachedLicenseInfo.MachineSignature != currentSignature)
                {
                    return LicenseStatus.MachineMismatch;
                }
                return LicenseStatus.Activated;
            }

            // Verify machine binding
            var machineSignature = _fingerprintService.GetMachineSignature();
            if (_cachedLicenseInfo.MachineSignature != machineSignature)
            {
                return LicenseStatus.MachineMismatch;
            }

            // Check for clock rollback (skip for newly created licenses)
            var now = DateTime.UtcNow;
            if (!_isNewlyCreated && now < _cachedLicenseInfo.LastRunDate.AddMinutes(-5)) // Allow 5 min tolerance
            {
                return LicenseStatus.ClockTampered;
            }

            // Check trial expiration
            var trialEndDate = _cachedLicenseInfo.TrialStartDate.AddDays(TrialDays);
            if (now > trialEndDate)
            {
                return LicenseStatus.TrialExpired;
            }

            return LicenseStatus.TrialActive;
        }

        /// <inheritdoc />
        public int GetRemainingTrialDays()
        {
            if (_cachedLicenseInfo == null)
                return 0;

            if (_cachedLicenseInfo.IsActivated)
                return -1; // Unlimited

            var trialEndDate = _cachedLicenseInfo.TrialStartDate.AddDays(TrialDays);
            var remaining = (trialEndDate - DateTime.UtcNow).Days;

            return Math.Max(0, remaining);
        }

        /// <inheritdoc />
        public bool IsFeatureAllowed(string featureName)
        {
            var status = GetLicenseStatus();

            // Always allow if activated or trial is active
            if (status == LicenseStatus.Activated || status == LicenseStatus.TrialActive)
            {
                return true;
            }

            // Check if feature is restricted
            return !RestrictedFeatures.Contains(featureName);
        }

        /// <inheritdoc />
        public void InitializeTrial()
        {
            // Always reload from storage to get the latest data
            _cachedLicenseInfo = _storageService.LoadLicenseInfo();

            if (_cachedLicenseInfo != null)
            {
                System.Diagnostics.Debug.WriteLine("Trial already initialized, using existing data.");
                return;
            }

            var now = DateTime.UtcNow;
            var licenseInfo = new LicenseInfo
            {
                TrialStartDate = now,
                LastRunDate = now,
                MachineSignature = _fingerprintService.GetMachineSignature(),
                IsActivated = false
            };

            _storageService.SaveLicenseInfo(licenseInfo);
            _cachedLicenseInfo = licenseInfo;
            _isNewlyCreated = true; // Mark as newly created to skip clock check this session

            System.Diagnostics.Debug.WriteLine($"Trial initialized. Start date: {now:O}");
        }

        /// <inheritdoc />
        public void RecordAppRun()
        {
            if (_cachedLicenseInfo == null)
            {
                InitializeTrial();
                return;
            }

            var now = DateTime.UtcNow;
            
            // Only update if a reasonable time has passed (avoid constant writes)
            if ((now - _cachedLicenseInfo.LastRunDate).TotalMinutes > 1)
            {
                _cachedLicenseInfo.LastRunDate = now;
                _storageService.SaveLicenseInfo(_cachedLicenseInfo);
                System.Diagnostics.Debug.WriteLine($"App run recorded. Last run: {now:O}");
            }
        }

        /// <inheritdoc />
        public bool ActivateLicense(string licenseKey)
        {
            // Simple license key validation
            // In production, this would call a server API
            if (string.IsNullOrEmpty(licenseKey))
                return false;

            // Basic format validation: XXXX-XXXX-XXXX-XXXX
            if (!IsValidLicenseKeyFormat(licenseKey))
                return false;

            // For demo purposes, accept any properly formatted key
            // In production, validate against server/database
            if (_cachedLicenseInfo == null)
            {
                InitializeTrial();
            }

            _cachedLicenseInfo!.IsActivated = true;
            _cachedLicenseInfo.LastRunDate = DateTime.UtcNow;
            _storageService.SaveLicenseInfo(_cachedLicenseInfo);

            System.Diagnostics.Debug.WriteLine("License activated successfully!");
            return true;
        }

        /// <inheritdoc />
        public string GetStatusMessage()
        {
            var status = GetLicenseStatus();
            return status switch
            {
                LicenseStatus.Activated => "Full Version",
                LicenseStatus.TrialActive => $"Trial: {GetRemainingTrialDays()} days remaining",
                LicenseStatus.TrialExpired => "Trial Expired - Please activate license",
                LicenseStatus.MachineMismatch => "License Error: Machine mismatch",
                LicenseStatus.ClockTampered => "License Error: System clock tampering detected",
                LicenseStatus.Invalid => "License Error: Invalid or corrupted data",
                _ => "Unknown Status"
            };
        }

        private static bool IsValidLicenseKeyFormat(string key)
        {
            // Format: XXXX-XXXX-XXXX-XXXX (alphanumeric)
            if (string.IsNullOrEmpty(key))
                return false;

            var parts = key.Split('-');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (part.Length != 4 || !part.All(char.IsLetterOrDigit))
                    return false;
            }

            return true;
        }

#if DEBUG
        /// <summary>
        /// DEBUG ONLY: Forces the trial to expire by setting the start date to 20 days ago.
        /// </summary>
        public void ForceTrialExpired()
        {
            if (_cachedLicenseInfo == null) InitializeTrial();
            
            _cachedLicenseInfo!.TrialStartDate = DateTime.UtcNow.AddDays(-20);
            _cachedLicenseInfo.LastRunDate = DateTime.UtcNow;
            _storageService.SaveLicenseInfo(_cachedLicenseInfo);
            _isNewlyCreated = false; // Allow clock checks now
            
            System.Diagnostics.Debug.WriteLine("DEBUG: Trial forced to expired state.");
        }

        /// <summary>
        /// DEBUG ONLY: Resets the trial by clearing all license data from storage.
        /// </summary>
        public void ResetTrial()
        {
            _storageService.ClearLicenseData();
            _cachedLicenseInfo = null;
            _isNewlyCreated = false;
            System.Diagnostics.Debug.WriteLine("DEBUG: Trial storage cleared and cache reset.");
        }
#endif
    }
}
