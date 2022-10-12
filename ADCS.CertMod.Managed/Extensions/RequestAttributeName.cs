namespace ADCS.CertMod.Managed.Extensions;

/// <summary>
/// Contains enumeration of request attributes.
/// </summary>
public enum RequestAttributeName {
    Challenge, // NETSCAPE
    ExpectedChallenge, // NETSCAPE
    Disposition,
    Deny,
    Pending,
    ValidityPeriod,
    ValidityPeriodUnits,
    ExpirationDate,
    CertType,
    CertificateTemplate,
    CertificateUsage,
    RequestOSVersion,
    RequestCSPProvider,
    CertFile,
    cbm,
    ccm,
    cdc,
    rmd,
    san,
    dns,
    dn,
    url,
    ipaddress,
    guid,
    oid,
    upn
}