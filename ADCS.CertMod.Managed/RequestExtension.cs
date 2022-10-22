using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents request extension entry in CA database.
/// </summary>
public class RequestExtension {
    readonly List<Byte> _rawData = new();

    internal RequestExtension(Int32 requestID, String oid, RequestExtensionFlags flags, IEnumerable<Byte> rawData) {
        RequestID = requestID;
        ExtensionName = new Oid(oid);
        Flags = flags;
        _rawData.AddRange(rawData);
    }

    /// <summary>
    /// Gets the request ID this extension is associated with.
    /// </summary>
    public Int32 RequestID { get; }
    /// <summary>
    /// Gets the extension OID and display name.
    /// </summary>
    public Oid ExtensionName { get; }
    /// <summary>
    /// Gets extension flags.
    /// </summary>
    public RequestExtensionFlags Flags { get; }
    /// <summary>
    /// Gets extension ASN.1-encoded raw data.
    /// </summary>
    public Byte[] Value => _rawData.ToArray();
}