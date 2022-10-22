namespace ADCS.CertMod.Managed.Extensions;

/// <summary>
/// Contains enumeration of request properties.
/// </summary>
enum RequestPropertyName {
    RequestID,
    RawRequest,
    RawArchivedKey,
    KeyRecoveryHashes,
    RawOldCertificate,
    RequestAttributes,
    RequestType,
    RequestFlags,
    StatusCode,
    Disposition,
    DispositionMessage,
    SubmittedWhen,
    ResolvedWhen,
    RevokedWhen,
    RevokedEffectiveWhen,
    RevokedReason,
    RequesterName,
    CallerName,
    SignerPolicies,
    SignerApplicationPolicies,
    Officer,
    PublishExpiredCertInCRL,
    AttestationChallenge,
    EndorsementKeyHash,
    EndorsementCertificateHash
}