using System;

namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents base implementation of <see cref="ICertManageModule"/> interface with default implementation
/// for optional methods. Certain members are abstract and must be implemented by inheritors.
/// </summary>
public abstract class CertManageModule : ICertManageModule {
    /// <inheritdoc />
    public abstract Object GetProperty(String strConfig, String strStorageLocation, String strPropertyName, Int32 Flags);
    /// <inheritdoc />
    public virtual void SetProperty(String strConfig, String strStorageLocation, String strPropertyName, Int32 Flags, ref Object pvarProperty) { }
    /// <inheritdoc />
    public virtual void Configure(String strConfig, String strStorageLocation, Int32 Flags) { }
}
