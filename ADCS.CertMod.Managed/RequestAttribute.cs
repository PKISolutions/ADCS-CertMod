using System;

namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents request attribute entry in CA database.
/// </summary>
public class RequestAttribute {
    internal RequestAttribute(Int32 requestID, String name, String value) {
        RequestID = requestID;
        Name = name;
        Value = value;
    }

    /// <summary>
    /// Gets the request ID this attribute is associated with.
    /// </summary>
    public Int32 RequestID { get; }
    /// <summary>
    /// Gets the attribute name.
    /// </summary>
    public String Name { get; }
    /// <summary>
    /// Gets the attribute value.
    /// </summary>
    public String Value { get; }
}