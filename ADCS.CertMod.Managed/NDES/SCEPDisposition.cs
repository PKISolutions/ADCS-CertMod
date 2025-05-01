namespace ADCS.CertMod.Managed.NDES;

/// <summary>
/// Represents SCEP disposition enumeration (request resolution status).
/// </summary>
public enum SCEPDisposition {
    /// <summary>
    /// SCEP request was successfully processed and issued.
    /// </summary>
    SCEPDispositionSuccess          = 0,
    /// <summary>
    /// SCEP request failed. See 'lastHResult' parameter for additional failure information.
    /// </summary>
    SCEPDispositionFailure          = 2,
    /// <summary>
    /// SCEP request was successfully processed, however was put in pending state for CA manager review.
    /// </summary>
    SCEPDispositionPending          = 3,
    /// <summary>
    /// Request is pending challenge.
    /// </summary>
    SCEPDispositionPendingChallenge = 11,
    /// <summary>
    /// Something is wrong.
    /// </summary>
    SCEPDispositionUnknown          = -1
}