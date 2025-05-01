namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents AD CS module type enumeration.
/// </summary>
public enum CertServerModuleType {
    /// <summary>
    /// Represents the Policy module type in AD CS, which is responsible for 
    /// implementing certificate issuance policies.
    /// </summary>
    Policy  = 1,
    /// <summary>
    /// Represents the Exit module type in the AD CS.
    /// </summary>
    Exit    = 2,
    /// <summary>
    /// Represents the Network Device Enrollment Service (NDES) module type in Active Directory Certificate Services (AD CS).
    /// </summary>
    NDES    = 5
}