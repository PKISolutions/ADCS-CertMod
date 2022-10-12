using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ADCS.CertMod.Managed.Exit;

namespace ADCS.CertMod.Managed.Policy;

/// <summary>
/// Provide communications between the Certificate Services server engine and the policy module.
/// </summary>
/// <seealso href="https://docs.microsoft.com/en-us/windows/win32/api/certpol/nn-certpol-icertpolicy2">ICertPolicy2</seealso>
[Guid("3db4910e-8001-4bf1-aa1b-f43a808317a0")] // from CertPol.idl
[TypeLibType(TypeLibTypeFlags.FDispatchable | TypeLibTypeFlags.FDual)]
[ComImport]
public interface ICertPolicy2 : ICertPolicy {
    /// <inheritdoc cref="ICertPolicy.Initialize"/>
    [DispId(0x60020000)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    new void Initialize([MarshalAs(UnmanagedType.BStr), In] String strConfig);

    /// <inheritdoc cref="ICertPolicy.VerifyRequest"/>
    [DispId(0x60020001)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    new PolicyModuleAction VerifyRequest(
        [MarshalAs(UnmanagedType.BStr), In] String strConfig,
        [In] Int32 Context,
        [In] Int32 bNewRequest,
        [In] Int32 Flags);

    /// <inheritdoc cref="ICertPolicy.GetDescription"/>
    [DispId(0x60020002)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [return: MarshalAs(UnmanagedType.BStr)]
    new String GetDescription();
    /// <inheritdoc cref="ICertPolicy.ShutDown"/>
    [DispId(0x60020003)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    new void ShutDown();

    /// <summary>
    /// Called by the server engine to retrieve the object that allows the Certification Authority to configure the policy module.
    /// </summary>
    /// <returns>An instance of <see cref="ICertManageModule"/>.</returns>
    /// <seealso href="https://docs.microsoft.com/windows/win32/api/certpol/nf-certpol-icertpolicy2-getmanagemodule">ICertPolicy2::GetManageModule</seealso>
    [DispId(0x60030000)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [return: MarshalAs(UnmanagedType.Interface)]
    ICertManageModule GetManageModule();
}