# MyShop Trial & Licensing System Documentation

## 1. Overview
The MyShop Trial System is a secure, offline-first licensing solution designed to allow users to evaluate the software for 15 days before requiring a full license activation. It features robust machine binding, encrypted data storage, and protection against system clock tampering.

## 2. Key Features
- **15-Day Trial Period**: Automatically initialized upon the first run of the application.
- **Machine Binding (Hardware Fingerprinting)**: Licenses are uniquely tied to the machine's hardware identifiers (CPU, Motherboard, and Disk).
- **Secure Dual-Storage**: License data is stored in the Windows Registry (primary) and an encrypted backup file in `%AppData%` (resilience).
- **Security & Encryption**: Uses AES-256 encryption with PBKDF2 key derivation and HMAC-SHA256 for integrity verification.
- **Clock Tampering Protection**: Detects if the system clock has been rolled back to fraud the trial period.
- **Feature Restrictions**: Automatically disables key functionalities (e.g., creating orders, adding products) once the trial expires.

## 3. Technical Architecture

### Core Components (MyShop.Core)
- `ILicenseService`: The primary interface for checking status, remaining days, and authorizing features.
- `LicenseInfo`: Data model containing trial dates, machine signature, and activation state.
- `LicenseStatus`: Enum representing the current state (TrialActive, TrialExpired, ClockTampered, MachineMismatch, etc.).

### Implementations (MyShop.Core/Services)
- `LicenseService`: Orchestrates the licensing logic (Internal business logic).
- `FingerprintService`: Generates a hardware-bound SHA256 signature using WMI queries.
- `CryptoHelper`: Handles AES-256 encryption/decryption and HMAC hashing.
- `SecureStorageService`: Manages data persistence in Registry and File System.

### UI Components (MyShop.App/Views)
- `ShellPage`: Displays trial status and handles activation dialogs.
- `ProductsScreen/OrdersPage/CustomersPage`: Enforce feature restrictions.

## 4. Security Implementation Detail
- **Key Derivation**: The encryption keys are not stored. They are derived at runtime from the unique Machine Signature using PBKDF2 with 10,000 iterations.
- **Integrity Check**: Every time license data is loaded, an HMAC hash is verified to ensure the data hasn't been manually edited in the Registry or file.
- **Clock Rollback Detection**: The app records the `LastRunDate` on every launch. If the current system time is earlier than the last recorded time, a "Clock Tampering" block is triggered.

## 5. UI Integration
- **Trial Banner**: Integrated into the `ShellPage` sidebar. Shows remaining days or expiration status.
- **Error Dialogs**: Automatic popups on startup for critical errors (Tampering, Mismatch, Expired).
- **Feature Blocking**: Any restricted action triggers a `ShowLicenseErrorDialog` if the license is not valid/active.

## 6. Testing & Development (DEBUG Mode)
The system includes specialized helpers for developers:
- **Secret Shortcut**: Double-click the Trial Banner in the sidebar to instantly force the trial to an **Expired** state.
- **Rescue Button**: If Clock Tampering is detected, a **"Reset License (Debug)"** button appears to clear all storage and start fresh.
- **Note**: These features are wrapped in `#if DEBUG` and are removed from **Release** builds.

## 7. Operational Procedures

### Activating a License
1. User clicks **"Enter License Key"**.
2. Format required: `XXXX-XXXX-XXXX-XXXX` (Alphanumeric).
3. Currently, the system accepts any valid-format key (Demo Mode). *Note: Production should implement server-side validation.*

### Resetting Trial (Manual)
If needed (e.g., after Release testing), run this PowerShell command to clear all license data:
```powershell
Remove-Item -Path "HKCU:\Software\MyShop\License" -Recurse -Force -ErrorAction SilentlyContinue; 
Remove-Item -Path "$env:APPDATA\MyShop\sys.bin" -Force -ErrorAction SilentlyContinue;
```

## 9. Key Generation Guide (for Administrators)

Since the licensing system is machine-bound, keys must be generated using the client's unique **Machine Signature**.

### Activation Logic Details
A valid license key follows the format `PREFIX-XXXX` where:
1. `PREFIX`: A random string (e.g., `MYSH-OP25-PRO`).
2. `XXXX`: A 4-character checksum bound to the machine.

**Mathematical Formula:**
`Checksum = First 4 characters of SHA256(PrefixWithoutDashes + MachineSignature)`

### Generation Steps
To generate a key for a customer:
1. Ask the customer to provide their **Machine ID** (found in the "Activate MyShop License" dialog).
2. Use the **Standalone Key Generator** tool (in tools/keygen).
3. Enter the Machine ID and choose a Prefix.
4. Send the resulting key to the customer.

## 10. Troubleshooting & Maintenance

- **Clock Tampering Blocked**: If a user is blocked due to clock tampering but asserts innocence, use the **Debug Reset** (Double-tap trial banner in Debug mode) to clear the state.
- **Machine Mismatch**: Occurs if hardware is significantly changed. The user must be issued a new key bound to the new signature.
- **Internet Time Failure**: The app requires an internet connection *occasionally* to verify the clock. If no internet is available for a long period, it persists with local checks but logs a warning in Debug.

