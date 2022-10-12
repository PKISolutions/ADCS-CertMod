namespace ADCS.CertMod.Managed.Extensions;

/// <summary>
/// Contains an enumeration of certificate properties available in CA database.
/// </summary>
public enum CertificatePropertyName {
    RequestID,
    RawCertificate,
    CertificateHash,
    CertificateTemplate,
    EnrollmentFlags,
    GeneralFlags,
    PrivateKeyFlags,
    SerialNumber,
    NotBefore,
    NotAfter,
    SubjectKeyIdentifier,
    RawPublicKey,
    PublicKeyLength,
    PublicKeyAlgorithm,
    RawPublicKeyAlgorithmParameters,
    UPN,
    RevokedEffectiveWhen
}