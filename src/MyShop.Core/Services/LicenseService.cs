using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShop.Core.Services
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
        private DateTime? _cachedRemoteTime;

        private const int TrialDays = 15;

        // Features that require active trial or full license
        private static readonly HashSet<string> RestrictedFeatures = new(StringComparer.OrdinalIgnoreCase)
        {
            "CreateOrder",
            "EditOrder",
            "CancelOrder",
            "AddProduct",
            "EditProduct",
            "DeleteProduct",
            "AddCategory",
            "EditCategory",
            "DeleteCategory",
            "AddCustomer",
            "EditCustomer",
            "DeleteCustomer",
            "ManageDiscounts"
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

            // Remote Clock Tampering Check (if cached)
            if (_cachedRemoteTime.HasValue && now < _cachedRemoteTime.Value.AddMinutes(-10))
            {
                var diff = _cachedRemoteTime.Value - now;
                System.Diagnostics.Debug.WriteLine($"CRITICAL: Local time is significantly behind remote time! Diff: {diff.TotalMinutes:N1} minutes. Local UTC: {now:O}, Remote UTC: {_cachedRemoteTime.Value:O}");
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
                
                // Trigger background remote time check if not done recently
                if (!_cachedRemoteTime.HasValue || (now - _cachedRemoteTime.Value).TotalHours > 1)
                {
                    _ = CheckRemoteTimeAsync();
                }
            }
        }

        private async System.Threading.Tasks.Task CheckRemoteTimeAsync()
        {
            try
            {
                using var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                // Using WorldTimeAPI (no key required)
                var response = await client.GetStringAsync("http://worldtimeapi.org/api/timezone/Etc/UTC");
                
                using var doc = System.Text.Json.JsonDocument.Parse(response);
                if (doc.RootElement.TryGetProperty("datetime", out var dtProp))
                {
                    var remoteTime = dtProp.GetDateTime();
                    _cachedRemoteTime = remoteTime;
                    System.Diagnostics.Debug.WriteLine($"Remote time check successful: {remoteTime:O}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Remote time check failed: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public bool ActivateLicense(string licenseKey)
        {
            if (string.IsNullOrEmpty(licenseKey))
                return false;

            // 1. Basic format validation
            if (!IsValidLicenseKeyFormat(licenseKey))
                return false;

            // 2. Machine-bound validation
            // The key must be mathematically tied to this specific machine's signature
            if (!VerifyMachineBoundKey(licenseKey))
            {
                System.Diagnostics.Debug.WriteLine("License activation failed: Key is not valid for this machine.");
                return false;
            }

            if (_cachedLicenseInfo == null)
            {
                InitializeTrial();
            }

            _cachedLicenseInfo!.IsActivated = true;
            _cachedLicenseInfo.LastRunDate = DateTime.UtcNow;
            _storageService.SaveLicenseInfo(_cachedLicenseInfo);

            System.Diagnostics.Debug.WriteLine("License activated successfully and bound to machine!");
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
            // Format: XXXX-XXXX-XXXX-XXXX (alphanumeric, uppercase)
            if (string.IsNullOrEmpty(key))
                return false;

            var parts = key.Split('-');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (part.Length != 4 || !part.All(c => char.IsLetterOrDigit(c)))
                    return false;
            }

            return true;
        }

        private bool VerifyMachineBoundKey(string key)
        {
            try
            {
                var parts = key.ToUpper().Split('-');
                var prefix = string.Join("", parts.Take(3)); // First 12 chars
                var providedChecksum = parts[3]; // Last 4 chars

                // Generate expected checksum: Hash(Prefix + MachineSignature)
                var machineSignature = _fingerprintService.GetMachineSignature();
                var rawData = prefix + machineSignature;
                
                // Using a simple but effective deterministic transform for the demo
                // In production, use a proper HMAC or specific logic from your keygen
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
                var hashString = BitConverter.ToString(hashBytes).Replace("-", "");
                
                // Take 4 chars from the hash as the checksum
                var expectedChecksum = hashString.Substring(0, 4);

                return providedChecksum == expectedChecksum;
            }
            catch
            {
                return false;
            }
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
            _cachedRemoteTime = null;
            System.Diagnostics.Debug.WriteLine("DEBUG: Trial storage cleared and cache reset.");
        }
#endif
    }
}
