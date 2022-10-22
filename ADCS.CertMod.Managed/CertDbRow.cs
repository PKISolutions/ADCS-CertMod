using System;
using System.Collections.Generic;

namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents a CA database row dictionary, where key represents column name and value represents column value.
/// </summary>
public class CertDbRow : Dictionary<String, Object> {
    /// <summary>
    /// Initializes a new instance of <strong>CertDbRow</strong> class.
    /// </summary>
    public CertDbRow() { }
    /// <summary>
    /// Initializes a new instance of <strong>CertDbRow</strong> class from existing properties.
    /// </summary>
    /// <param name="dictionary">Existing DB properties.</param>
    public CertDbRow(IDictionary<String, Object> dictionary) : base(dictionary) { }

    /// <summary>
    /// Copies existing dictionary into current object.
    /// </summary>
    /// <param name="dictionary">Existing dictionary to copy values from.</param>
    /// <exception cref="ArgumentException">An element with the same key already exists in the dictionary.</exception>
    public void AddRange(IDictionary<String, Object> dictionary) {
        if (dictionary == null) {
            return;
        }
        foreach (KeyValuePair<String, Object> name in dictionary) {
            Add(name.Key, name.Value);
        }
    }
}