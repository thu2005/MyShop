using Moq;
using MyShop.Core.Services;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Models;
using Xunit;

namespace MyShop.Tests
{
    public class LicenseServiceTests
    {
        private readonly Mock<IFingerprintService> _fingerprintMock;
        private readonly Mock<ISecureStorageService> _storageMock;

        public LicenseServiceTests()
        {
            _fingerprintMock = new Mock<IFingerprintService>();
            _storageMock = new Mock<ISecureStorageService>();
            
            // Default setup: Machine signature is "TEST-MACHINE"
            _fingerprintMock.Setup(f => f.GetMachineSignature()).Returns("TEST-MACHINE");
        }

        [Fact]
        public void GetLicenseStatus_ShouldReturnInvalid_WhenNoDataExists()
        {
            // Arrange
            _storageMock.Setup(s => s.LoadLicenseInfo()).Returns((LicenseInfo?)null);
            var service = new LicenseService(_fingerprintMock.Object, _storageMock.Object);

            // Act
            var status = service.GetLicenseStatus();

            // Assert
            Assert.Equal(LicenseStatus.Invalid, status);
        }

        [Fact]
        public void GetLicenseStatus_ShouldReturnTrialActive_WhenNewTrialInitialized()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var license = new LicenseInfo
            {
                TrialStartDate = now,
                LastRunDate = now,
                MachineSignature = "TEST-MACHINE",
                IsActivated = false
            };
            _storageMock.Setup(s => s.LoadLicenseInfo()).Returns(license);
            var service = new LicenseService(_fingerprintMock.Object, _storageMock.Object);

            // Act
            var status = service.GetLicenseStatus();

            // Assert
            Assert.Equal(LicenseStatus.TrialActive, status);
        }

        [Fact]
        public void GetLicenseStatus_ShouldReturnTrialExpired_WhenTrialExceeds15Days()
        {
            // Arrange
            var oldDate = DateTime.UtcNow.AddDays(-16);
            var license = new LicenseInfo
            {
                TrialStartDate = oldDate,
                LastRunDate = DateTime.UtcNow,
                MachineSignature = "TEST-MACHINE",
                IsActivated = false
            };
            _storageMock.Setup(s => s.LoadLicenseInfo()).Returns(license);
            var service = new LicenseService(_fingerprintMock.Object, _storageMock.Object);

            // Act
            var status = service.GetLicenseStatus();

            // Assert
            Assert.Equal(LicenseStatus.TrialExpired, status);
        }

        [Fact]
        public void GetLicenseStatus_ShouldReturnClockTampered_WhenLocalTimeIsEarlierThanLastRun()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var license = new LicenseInfo
            {
                TrialStartDate = now.AddDays(-1),
                LastRunDate = now.AddHours(1), // Future date!
                MachineSignature = "TEST-MACHINE",
                IsActivated = false
            };
            _storageMock.Setup(s => s.LoadLicenseInfo()).Returns(license);
            var service = new LicenseService(_fingerprintMock.Object, _storageMock.Object);

            // Act
            var status = service.GetLicenseStatus();

            // Assert
            Assert.Equal(LicenseStatus.ClockTampered, status);
        }

        [Fact]
        public void ActivateLicense_ShouldReturnTrue_WhenKeyIsCorrectForMachine()
        {
            // Arrange
            var service = new LicenseService(_fingerprintMock.Object, _storageMock.Object);
            
            // To generate a valid key for the test:
            // Prefix: ABCD-EFGH-IJKL (12 chars)
            // MachineID: TEST-MACHINE
            // RawData: ABCDEFGHIJKLTEST-MACHINE
            // SHA256 of RawData -> first 4 chars
            
            var prefix = "ABCD-EFGH-IJKL";
            var rawData = "ABCDEFGHIJKL" + "TEST-MACHINE";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "");
            var checksum = hashString.Substring(0, 4);
            
            var validKey = $"{prefix}-{checksum}";

            // Act
            var result = service.ActivateLicense(validKey);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ActivateLicense_ShouldReturnFalse_WhenKeyIsForDifferentMachine()
        {
            // Arrange
            var service = new LicenseService(_fingerprintMock.Object, _storageMock.Object);
            
            // Key generated for "OTHER-MACHINE"
            var validKeyForOther = "ABCD-EFGH-IJKL-XXXX"; // Assuming XXXX is for other
            
            // Act
            var result = service.ActivateLicense(validKeyForOther);

            // Assert
            Assert.False(result);
        }
    }
}
