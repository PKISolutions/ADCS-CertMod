namespace ADCS.CertMod.Managed.Policy;

/// <summary>
/// Contains enumeration of possible actions CA may take on request submission.
/// </summary>
public enum PolicyModuleAction {
    /// <summary>
    /// Instructs CA to issue the certificate.
    /// </summary>
    PutToPending = 0,
    /// <summary>
    /// Instructs CA to issue the certificate.
    /// </summary>
    Issue        = 1,
    /// <summary>
    /// Instructs CA to permanently deny request.
    /// </summary>
    Deny         = 2
}