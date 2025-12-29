namespace MyShop.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for generating machine fingerprints.
    /// </summary>
    public interface IFingerprintService
    {
        /// <summary>
        /// Generates a unique machine signature based on hardware identifiers.
        /// </summary>
        /// <returns>SHA256 hash of combined hardware identifiers.</returns>
        string GetMachineSignature();
    }
}
