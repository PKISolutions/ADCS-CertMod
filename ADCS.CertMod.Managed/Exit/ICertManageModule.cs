using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ADCS.CertMod.Managed.Exit;

/// <summary>
/// The <strong>ICertManageModule</strong> interface is provided to retrieve information about a
/// Certificate Services Policy or Exit module.
/// </summary>
/// <seealso href="https://learn.microsoft.com/windows/win32/api/certmod/nn-certmod-icertmanagemodule">ICertManageModule</seealso>
[TypeLibType(TypeLibTypeFlags.FDispatchable | TypeLibTypeFlags.FDual)]
[Guid("e7d7ad42-bd3d-11d1-9a4d-00c04fc297eb")]
[ComImport]
public interface ICertManageModule {
    /// <summary>
    /// The GetProperty method retrieves a module's property value.
    /// </summary>
    /// <param name="strConfig">
    /// Represents the configuration string for the Certificate Services server in the form COMPUTERNAME\CANAME,
    /// where COMPUTERNAME is the Certificate Services server's network name, and CANAME is the common name of
    /// the certification authority (CA) as entered for the CA during Certificate Services setup.
    /// </param>
    /// <param name="strStorageLocation">
    /// A registry key that denotes the storage location in the HKEY_LOCAL_MACHINE hive for the property values.
    /// </param>
    /// <param name="strPropertyName">
    /// The name of the property being queried. Policy and exit modules should support the following properties.
    /// <list type="bullet">
    ///     <item><strong>Name</strong> - Name of the module.</item>
    ///     <item><strong>Description</strong> - Description of the module.</item>
    ///     <item><strong>Copyright</strong> - Copyright pertaining to the module.</item>
    ///     <item><strong>File Version</strong> - Version of the module file.</item>
    ///     <item><strong>Product Version</strong> - Version of the module.</item>
    /// </list>
    /// </param>
    /// <param name="Flags">This parameter is reserved and must be set to zero.</param>
    /// <returns>A value associated with the requested property.</returns>
    /// <seealso href="https://learn.microsoft.com/windows/win32/api/certmod/nf-certmod-icertmanagemodule-getproperty">ICertManageModule::GetProperty</seealso>
    [DispId(0x60020000)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [return: MarshalAs(UnmanagedType.Struct)]
    Object GetProperty(
        [MarshalAs(UnmanagedType.BStr), In] String strConfig,
        [MarshalAs(UnmanagedType.BStr), In] String strStorageLocation,
        [MarshalAs(UnmanagedType.BStr), In] String strPropertyName,
        [In] Int32 Flags);

    /// <summary>
    /// The SetProperty method allows a module to set a property value.
    /// </summary>
    /// <param name="strConfig">
    /// Represents the configuration string for the Certificate Services server in the form COMPUTERNAME\CANAME,
    /// where COMPUTERNAME is the Certificate Services server's network name, and CANAME is the common name of
    /// the certification authority (CA) as entered for the CA during Certificate Services setup.
    /// </param>
    /// <param name="strStorageLocation">
    /// A registry key that denotes the storage location in the HKEY_LOCAL_MACHINE hive for the property values.
    /// </param>
    /// <param name="strPropertyName">
    /// The name of the property being assigned. Policy and exit modules should support the following properties.
    /// <list type="bullet">
    ///     <item><strong>Name</strong> - Name of the module.</item>
    ///     <item><strong>Description</strong> - Description of the module.</item>
    ///     <item><strong>Copyright</strong> - Copyright pertaining to the module.</item>
    ///     <item><strong>File Version</strong> - Version of the module file.</item>
    ///     <item><strong>Product Version</strong> - Version of the module.</item>
    /// </list>
    /// </param>
    /// <param name="Flags">This parameter is reserved and must be set to zero.</param>
    /// <param name="pvarProperty">A value associated with the property being assigned.</param>
    /// <seealso href="https://learn.microsoft.com/windows/win32/api/certmod/nf-certmod-icertmanagemodule-setproperty">ICertManageModule::SetProperty</seealso>
    [DispId(0x60020001)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetProperty(
        [MarshalAs(UnmanagedType.BStr), In] String strConfig,
        [MarshalAs(UnmanagedType.BStr), In] String strStorageLocation,
        [MarshalAs(UnmanagedType.BStr), In] String strPropertyName,
        [In] Int32 Flags,
        [MarshalAs(UnmanagedType.Struct), In] ref Object pvarProperty);

    /// <summary>
    /// Displays the module user interface.
    /// </summary>
    /// <param name="strConfig">
    /// Represents the configuration string for the Certificate Services server in the form COMPUTERNAME\CANAME,
    /// where COMPUTERNAME is the Certificate Services server's network name, and CANAME is the common name of
    /// the certification authority (CA) as entered for the CA during Certificate Services setup.
    /// </param>
    /// <param name="strStorageLocation">
    /// A location that provides storage for the property values, as described in the definition of strStorageLocation in
    /// <i>strStorageLocation</i> in <see cref="GetProperty"/>.
    /// </param>
    /// <param name="Flags">
    /// A value used to determine whether the configuration interface is to be presented to the user. If this value is zero,
    /// the user will be presented with an interface for configuring the module. If this value is CMM_REFRESHONLY,
    /// Certificate Services will not display the user interface, but the latest changes to the configuration of the module
    /// will be in effect when future certificate requests are processed (this allows changes to be incorporated without
    /// requiring a response to a user interface).</param>
    /// <seealso href="https://learn.microsoft.com/windows/win32/api/certmod/nf-certmod-icertmanagemodule-configure">ICertManageModule::Configure</seealso>
    [DispId(0x60020002)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Configure(
        [MarshalAs(UnmanagedType.BStr), In] String strConfig,
        [MarshalAs(UnmanagedType.BStr), In] String strStorageLocation,
        [In] Int32 Flags);
}