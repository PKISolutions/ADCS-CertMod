using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ADCS.CertMod.Managed.Policy;

/// <summary>
/// Provide communications between the Certificate Services server engine and the policy module.
/// </summary>
/// <seealso href="https://learn.microsoft.com/windows/win32/api/certpol/nn-certpol-icertpolicy">ICertPolicy</seealso>
[Guid("38bb5a00-7636-11d0-b413-00a0c91bbf8c")] // from CertPol.idl
[TypeLibType(TypeLibTypeFlags.FDispatchable | TypeLibTypeFlags.FDual)]
[ComImport]
public interface ICertPolicy {
    /// <summary>
    /// Called by the server engine to allow the policy module to perform initialization tasks.
    /// </summary>
    /// <param name="strConfig">
    /// Represents the name of the certification authority, as entered during Certificate Services setup.
    /// </param>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/certpol/nf-certpol-icertpolicy-initialize">ICertPolicy::Initialize</seealso>
    [DispId(0x60020000)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Initialize([MarshalAs(UnmanagedType.BStr), In] String strConfig);

    /// <summary>
    /// The VerifyRequest method notifies the policy module that a new request has entered the system.
    /// The policy module can then interact with that request by passing Context as a parameter when
    /// retrieving or setting properties on the request or associated certificate.
    /// <para>
    /// The returned disposition value indicates whether the request has been accepted, denied, or has
    /// been sent to the administration queue for later evaluation.
    /// </para>
    /// </summary>
    /// <param name="strConfig">
    /// Represents the name of the certification authority, as entered during Certificate Services setup.
    /// </param>
    /// <param name="Context">
    /// Identifies the request and associated certificate under construction.
    /// The certificate server passes the context to this method.
    /// </param>
    /// <param name="bNewRequest">
    /// If set to 1, specifies that the request is new. If set to 0, the request is being resubmitted to the
    /// policy module as a result of an ICertAdmin::ResubmitRequest call. A value of FALSE can be used to
    /// indicate that the administrator wants the request to be issued or that request properties set by the
    /// administrator should be examined.
    /// </param>
    /// <param name="Flags">This parameter is reserved and must be set to zero.</param>
    /// <returns>A disposition value.</returns>
    /// <seealso href="https://learn.microsoft.com/windows/win32/api/certpol/nf-certpol-icertpolicy-verifyrequest">ICertPolicy::VerifyRequest</seealso>
    [DispId(0x60020001)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    PolicyModuleAction VerifyRequest(
        [MarshalAs(UnmanagedType.BStr), In] String strConfig,
        [In] Int32 Context,
        [In] Int32 bNewRequest,
        [In] Int32 Flags);

    /// <summary>
    /// Returns a human-readable description of the policy module and its function.
    /// </summary>
    /// <returns>A string that describes the policy module.</returns>
    /// <seealso href="https://learn.microsoft.com/windows/win32/api/certpol/nf-certpol-icertpolicy-getdescription">ICertPolicy::GetDescription</seealso>
    [DispId(0x60020002)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [return: MarshalAs(UnmanagedType.BStr)]
    String GetDescription();

    /// <summary>
    /// Called by the server engine before the server is terminated.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/windows/win32/api/certpol/nf-certpol-icertpolicy-shutdown">ICertPolicy::ShutDown</seealso>
    [DispId(0x60020003)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void ShutDown();
}