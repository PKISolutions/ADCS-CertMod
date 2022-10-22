namespace ADCS.CertMod.Managed;

/// <summary>
/// Contains enumeration of possible disposition statuses.
/// </summary>
public enum RequestDisposition {
    /// <summary>
    /// Request is being processed.
    /// </summary>
    Active      = 8,
    /// <summary>
    /// Request is placed into pending state for manual review by CA manager.
    /// </summary>
    Pending     = 9,
    /// <summary>
    /// Certificate is foreign and was imported to CA database.
    /// </summary>
    Foreign     = 12,
    /// <summary>
    /// Request row is CA certificate.
    /// </summary>
    CaCert      = 15,
    /// <summary>
    /// Request row is CA certificate and its chain (in PKCS#7 format).
    /// </summary>
    CaCertChain = 16,
    /// <summary>
    /// Request row is key recovery agent (KRA) certificate,
    /// </summary>
    KraCert     = 17,
    /// <summary>
    /// Certificate is issued.
    /// </summary>
    Issued      = 20,
    /// <summary>
    /// Certificate is revoked.
    /// </summary>
    Revoked     = 21,
    /// <summary>
    /// Request is failed.
    /// </summary>
    Failed      = 30,
    /// <summary>
    /// Request is denied by an administrator.
    /// </summary>
    Denied      = 31
}