using Microsoft.Win32;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using System;
using System.IO;
using System.Text.Json;

namespace MyShop.Core.Services
{
    /// <summary>
    /// Provides secure dual-storage for license data (Registry + AppData backup).
    /// </summary>
    public class SecureStorageService : ISecureStorageService
    {
        private readonly ICryptoHelper _cryptoHelper;
        private const string RegistryPath = @"Software\MyShop\License";
        private const string RegistryValueName = "LicenseData";
        private const string BackupFileName = "sys.bin";

        private readonly string _backupFilePath;

        public SecureStorageService(ICryptoHelper cryptoHelper)
        {
            _cryptoHelper = cryptoHelper;

            // Setup backup file path
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var myShopDir = Path.Combine(appDataPath, "MyShop");
            
            if (!Directory.Exists(myShopDir))
            {
                Directory.CreateDirectory(myShopDir);
            }
            
            _backupFilePath = Path.Combine(myShopDir, BackupFileName);
        }

        /// <inheritdoc />
        public void SaveLicenseInfo(LicenseInfo licenseInfo)
        {
            try
            {
                // Serialize and compute hash
                var jsonData = JsonSerializer.Serialize(new LicenseInfoDto
                {
                    TrialStartDate = licenseInfo.TrialStartDate,
                    LastRunDate = licenseInfo.LastRunDate,
                    MachineSignature = licenseInfo.MachineSignature,
                    IsActivated = licenseInfo.IsActivated
                });

                licenseInfo.DataHash = _cryptoHelper.ComputeHash(jsonData);

                // Include hash in final data
                var fullData = JsonSerializer.Serialize(licenseInfo);
                var encryptedData = _cryptoHelper.Encrypt(fullData);

                // Save to primary storage (Registry)
                SaveToRegistry(encryptedData);

                // Save to backup storage (AppData file)
                SaveToBackupFile(encryptedData);

                System.Diagnostics.Debug.WriteLine("License info saved successfully to both storage locations.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving license info: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc />
        public LicenseInfo? LoadLicenseInfo()
        {
            LicenseInfo? licenseInfo = null;

            // Try primary storage first
            licenseInfo = LoadFromRegistry();

            // If primary failed, try backup
            if (licenseInfo == null)
            {
                System.Diagnostics.Debug.WriteLine("Primary storage failed, trying backup...");
                licenseInfo = LoadFromBackupFile();

                // If backup succeeded, restore to primary
                if (licenseInfo != null)
                {
                    System.Diagnostics.Debug.WriteLine("Recovered from backup, restoring primary storage.");
                    SaveLicenseInfo(licenseInfo);
                }
            }

            // Verify data integrity
            if (licenseInfo != null && !VerifyDataIntegrity(licenseInfo))
            {
                System.Diagnostics.Debug.WriteLine("License data integrity check failed!");
                return null;
            }

            return licenseInfo;
        }

        /// <inheritdoc />
        public bool HasLicenseData()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
                if (key?.GetValue(RegistryValueName) != null)
                    return true;

                return File.Exists(_backupFilePath);
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public void ClearLicenseData()
        {
            try
            {
                // Clear registry
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: true);
                key?.DeleteValue(RegistryValueName, throwOnMissingValue: false);

                // Clear backup file
                if (File.Exists(_backupFilePath))
                {
                    File.Delete(_backupFilePath);
                }

                System.Diagnostics.Debug.WriteLine("License data cleared from all storage locations.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing license data: {ex.Message}");
            }
        }

        private void SaveToRegistry(string encryptedData)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
                key?.SetValue(RegistryValueName, encryptedData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registry save error: {ex.Message}");
                // Don't throw - backup might still succeed
            }
        }

        private void SaveToBackupFile(string encryptedData)
        {
            try
            {
                File.WriteAllText(_backupFilePath, encryptedData);

                // Set file as hidden
                var fileInfo = new FileInfo(_backupFilePath);
                fileInfo.Attributes |= FileAttributes.Hidden;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Backup file save error: {ex.Message}");
            }
        }

        private LicenseInfo? LoadFromRegistry()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
                var encryptedData = key?.GetValue(RegistryValueName) as string;

                if (string.IsNullOrEmpty(encryptedData))
                    return null;

                var decryptedData = _cryptoHelper.Decrypt(encryptedData);
                if (string.IsNullOrEmpty(decryptedData))
                    return null;

                return JsonSerializer.Deserialize<LicenseInfo>(decryptedData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registry load error: {ex.Message}");
                return null;
            }
        }

        private LicenseInfo? LoadFromBackupFile()
        {
            try
            {
                if (!File.Exists(_backupFilePath))
                    return null;

                var encryptedData = File.ReadAllText(_backupFilePath);
                if (string.IsNullOrEmpty(encryptedData))
                    return null;

                var decryptedData = _cryptoHelper.Decrypt(encryptedData);
                if (string.IsNullOrEmpty(decryptedData))
                    return null;

                return JsonSerializer.Deserialize<LicenseInfo>(decryptedData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Backup file load error: {ex.Message}");
                return null;
            }
        }

        private bool VerifyDataIntegrity(LicenseInfo licenseInfo)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(new LicenseInfoDto
                {
                    TrialStartDate = licenseInfo.TrialStartDate,
                    LastRunDate = licenseInfo.LastRunDate,
                    MachineSignature = licenseInfo.MachineSignature,
                    IsActivated = licenseInfo.IsActivated
                });

                return _cryptoHelper.VerifyHash(jsonData, licenseInfo.DataHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// DTO for hashing purposes (excludes the DataHash field).
        /// </summary>
        private class LicenseInfoDto
        {
            public DateTime TrialStartDate { get; set; }
            public DateTime LastRunDate { get; set; }
            public string MachineSignature { get; set; } = string.Empty;
            public bool IsActivated { get; set; }
        }
    }
}
