using System;

namespace ADCS.CertMod.Managed.Exit;

[Flags]
public enum ExitEvents {
    Invalid = 0x0,
    /// <summary>
    /// Fired when new certificate is issued (either, manually, or after CA Manager approval)
    /// </summary>
    CertIssued = 0x1,
    /// <summary>
    /// Fired when request was accepted and is pending for CA Manager approval.
    /// </summary>
    CertPending = 0x2,
    /// <summary>
    /// Fired when request failed or denied.
    /// </summary>
    CertDenied = 0x4,
    /// <summary>
    /// Fired for both, revoke and unrevoke (remove from Hold).
    /// </summary>
    CertRevoked = 0x8,
    /// <summary>
    /// It seems this event is never fired.
    /// </summary>
    CertRetrievePending = 0x10,
    /// <summary>
    /// Fired when Base or Delta CRL is published or republished. Once per ICertAdmin2::PublishCRL call.
    /// </summary>
    CRLIssued = 0x20,
    /// <summary>
    /// Fired when CA service is stopping.
    /// </summary>
    Shutdown = 0x40,
    /// <summary>
    /// It seems this event is never fired.
    /// </summary>
    ServiceStart = 0x80,
    /// <summary>
    /// It seems this event is never fired. Instead, CertRevoked is used.
    /// </summary>
    CertUnrevoked = 0x100,
    /// <summary>
    /// Fired when foreign certificate is imported into CA database.
    /// </summary>
    CertImported = 0x200,
    AllEvents = CertIssued | CertPending | CertDenied | CertRevoked | CertRetrievePending
                | CRLIssued | Shutdown | ServiceStart | CertUnrevoked | CertImported
}