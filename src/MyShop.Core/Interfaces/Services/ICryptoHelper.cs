namespace MyShop.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for cryptographic operations.
    /// </summary>
    public interface ICryptoHelper
    {
        /// <summary>
        /// Encrypts data using AES-256 with the machine-specific key.
        /// </summary>
        /// <param name="plainText">The data to encrypt.</param>
        /// <returns>Base64-encoded encrypted data with IV.</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypts data that was encrypted with the Encrypt method.
        /// </summary>
        /// <param name="cipherText">Base64-encoded encrypted data.</param>
        /// <returns>Decrypted plaintext, or null if decryption fails.</returns>
        string? Decrypt(string cipherText);

        /// <summary>
        /// Computes HMAC-SHA256 hash for data integrity verification.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>Base64-encoded HMAC hash.</returns>
        string ComputeHash(string data);

        /// <summary>
        /// Verifies the HMAC hash of data.
        /// </summary>
        /// <param name="data">The data to verify.</param>
        /// <param name="hash">The expected hash.</param>
        /// <returns>True if hash matches, false otherwise.</returns>
        bool VerifyHash(string data, string hash);
    }
}
