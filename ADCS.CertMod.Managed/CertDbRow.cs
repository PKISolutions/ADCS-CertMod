using System;
using System.Collections.Generic;

namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents a CA database row dictionary, where key represents column name and value represents column value.
/// </summary>
public class CertDbRow : Dictionary<String, Object> {
    public CertDbRow() { }
    public CertDbRow(IDictionary<String, Object> dictionary) : base(dictionary) { }

    /// <summary>
    /// Copies existing dictionary into current object.
    /// </summary>
    /// <param name="dictionary">Existing dictionary to copy values from.</param>
    public void AddRange(IDictionary<String, Object> dictionary) {
        if (dictionary == null) {
            return;
        }
        foreach (KeyValuePair<String, Object> name in dictionary) {
            Add(name.Key, name.Value);
        }
    }
}