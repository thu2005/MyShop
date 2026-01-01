using MyShop.Core.Interfaces.Services;
using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace MyShop.Core.Services
{
    /// <summary>
    /// Generates unique machine fingerprints using hardware identifiers.
    /// </summary>
    public class FingerprintService : IFingerprintService
    {
        private string? _cachedSignature;

        /// <inheritdoc />
        public string GetMachineSignature()
        {
            if (!string.IsNullOrEmpty(_cachedSignature))
            {
                return _cachedSignature;
            }

            try
            {
                var cpuId = GetWmiProperty("Win32_Processor", "ProcessorId");
                var motherboardSerial = GetWmiProperty("Win32_BaseBoard", "SerialNumber");
                var diskSerial = GetWmiProperty("Win32_DiskDrive", "SerialNumber");

                // Combine hardware identifiers
                var combinedId = $"CPU:{cpuId}|MB:{motherboardSerial}|DISK:{diskSerial}";

                // Hash the combined identifier
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedId));
                _cachedSignature = Convert.ToBase64String(hashBytes);

                return _cachedSignature;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating machine signature: {ex.Message}");
                // Fallback to a simpler identifier
                return GenerateFallbackSignature();
            }
        }

        private static string GetWmiProperty(string wmiClass, string propertyName)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {wmiClass}");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var value = obj[propertyName]?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WMI query failed for {wmiClass}.{propertyName}: {ex.Message}");
            }

            return "UNKNOWN";
        }

        private string GenerateFallbackSignature()
        {
            // Use machine name and user domain as fallback
            var fallbackId = $"{Environment.MachineName}|{Environment.UserDomainName}|{Environment.OSVersion}";
            
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(fallbackId));
            _cachedSignature = Convert.ToBase64String(hashBytes);
            
            return _cachedSignature;
        }
    }
}
