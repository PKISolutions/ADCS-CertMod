using System;

namespace ADCS.CertMod.Managed.Exit;

/// <summary>
/// Contains enumeration of possible exit events.
/// <para>This enumeration has a <see cref="FlagsAttribute"/> attribute that allows a bitwise combination of its member values.</para>
/// </summary>
[Flags]
public enum ExitEvents {
    /// <summary>
    /// Invalid
    /// </summary>
    None       = 0x0,
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
    /// <summary>
    /// All events.
    /// </summary>
    AllEvents = CertIssued | CertPending | CertDenied | CertRevoked | CertRetrievePending
                | CRLIssued | Shutdown | ServiceStart | CertUnrevoked | CertImported
}