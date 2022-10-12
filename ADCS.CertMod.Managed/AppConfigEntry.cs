using System;

namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents exit or policy module configuration entry.
/// </summary>
public class AppConfigEntry {

    public AppConfigEntry(String name, Object value, Type type = null) {
        Name = name;
        Value = value;
        ValueType = type;
    }
    /// <summary>
    /// Gets or sets the configuration entry name.
    /// </summary>
    public String Name { get; set; }
    /// <summary>
    /// Gets or sets the CLR type of configuration entry value.
    /// </summary>
    public Type ValueType { get; set; }
    /// <summary>
    /// Gets or sets the configuration entry value.
    /// </summary>
    public Object Value { get; set; }
}