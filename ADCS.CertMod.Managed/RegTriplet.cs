using System;
using System.Security;
using ADCS.CertMod.Managed.Extensions;
using Microsoft.Win32;

namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents managed registry key value.
/// </summary>
public class RegTriplet {
    /// <summary>
    /// Initializes a new instance of <strong>RegTriplet</strong> class from entry name and value type.
    /// </summary>
    /// <param name="name">Registry value name.</param>
    /// <param name="type">Registry value type.</param>
    public RegTriplet(String name, RegistryValueKind type) {
        Name = name;
        Type = type;
    }
    /// <summary>
    /// Initializes a new instance of <strong>RegTriplet</strong> class from app config entry.
    /// </summary>
    /// <param name="entry"></param>
    /// <exception cref="ArgumentNullException"><strong>entry</strong> parameter is null.</exception>
    /// <exception cref="ArgumentException">entry name is null or empty string.</exception>
    public RegTriplet(AppConfigEntry entry) {
        if (entry == null) {
            throw new ArgumentNullException(nameof(entry));
        }
        if (String.IsNullOrEmpty(entry.Name)) {
            throw new ArgumentException("Configuration entry name cannot be empty.");
        }
        Name = entry.Name;
        Value = entry.Value;
        if (entry.ValueType == typeof(Boolean)) {
            Type = RegistryValueKind.DWord;
            Value = (Boolean)entry.Value ? 1 : 0;
        }
        if (entry.ValueType == typeof(String)) {
            Type = RegistryValueKind.String;
            return;
        }
        if (entry.ValueType == typeof(String[])) {
            Type = RegistryValueKind.MultiString;
            return;
        }
        if (entry.ValueType == typeof(Int32) || entry.ValueType == typeof(UInt32)) {
            Type = RegistryValueKind.DWord;
            return;
        }
        if (entry.ValueType == typeof(Int64) || entry.ValueType == typeof(UInt64)) {
            Type = RegistryValueKind.QWord;
            return;
        }
        if (entry.ValueType == typeof(Byte[])) {
            Type = RegistryValueKind.Binary;
        }
        if (entry.ValueType == typeof(SecureString)) {
            Type = RegistryValueKind.String;
            Value = ((SecureString)entry.Value).EncryptPassword();
        }
    }

    /// <summary>
    /// Gets the registry value name.
    /// </summary>
    public String Name { get; }
    /// <summary>
    /// Gets or sets the registry value data.
    /// </summary>
    public Object? Value { get; set; }
    /// <summary>
    /// Gets registry value type.
    /// </summary>
    public RegistryValueKind Type { get; }
}