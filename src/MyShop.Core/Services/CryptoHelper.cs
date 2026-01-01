using MyShop.Core.Interfaces.Services;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MyShop.Core.Services
{
    /// <summary>
    /// Provides AES-256-CBC encryption and HMAC-SHA256 hashing for license protection.
    /// </summary>
    public class CryptoHelper : ICryptoHelper
    {
        private readonly byte[] _encryptionKey;
        private readonly byte[] _hmacKey;
        private const int KeySize = 32; // 256 bits
        private const int IvSize = 16;  // 128 bits
        private const int Iterations = 100000;

        public CryptoHelper(IFingerprintService fingerprintService)
        {
            // Derive encryption and HMAC keys from machine signature
            var machineSignature = fingerprintService.GetMachineSignature();
            var salt = Encoding.UTF8.GetBytes("MyShop_License_Salt_v2");

            using var deriveBytes = new Rfc2898DeriveBytes(
                machineSignature,
                salt,
                Iterations,
                HashAlgorithmName.SHA256);

            _encryptionKey = deriveBytes.GetBytes(KeySize);
            _hmacKey = deriveBytes.GetBytes(KeySize);
        }

        /// <inheritdoc />
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                using var aes = Aes.Create();
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = _encryptionKey;
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor();
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // Combine IV + encrypted data
                var resultBytes = new byte[IvSize + encryptedBytes.Length];
                Buffer.BlockCopy(aes.IV, 0, resultBytes, 0, IvSize);
                Buffer.BlockCopy(encryptedBytes, 0, resultBytes, IvSize, encryptedBytes.Length);

                return Convert.ToBase64String(resultBytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Encryption error: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc />
        public string? Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return null;

            try
            {
                var cipherBytes = Convert.FromBase64String(cipherText);
                
                if (cipherBytes.Length < IvSize)
                    return null;

                using var aes = Aes.Create();
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = _encryptionKey;

                // Extract IV from the beginning of cipher data
                var iv = new byte[IvSize];
                Buffer.BlockCopy(cipherBytes, 0, iv, 0, IvSize);
                aes.IV = iv;

                // Extract encrypted data
                var encryptedBytes = new byte[cipherBytes.Length - IvSize];
                Buffer.BlockCopy(cipherBytes, IvSize, encryptedBytes, 0, encryptedBytes.Length);

                using var decryptor = aes.CreateDecryptor();
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Decryption error: {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc />
        public string ComputeHash(string data)
        {
            if (string.IsNullOrEmpty(data))
                return string.Empty;

            using var hmac = new HMACSHA256(_hmacKey);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var hashBytes = hmac.ComputeHash(dataBytes);
            return Convert.ToBase64String(hashBytes);
        }

        /// <inheritdoc />
        public bool VerifyHash(string data, string hash)
        {
            if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(hash))
                return false;

            var computedHash = ComputeHash(data);
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(computedHash),
                Convert.FromBase64String(hash));
        }
    }
}
