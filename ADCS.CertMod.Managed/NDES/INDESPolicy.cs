using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ADCS.CertMod.Managed.NDES;

/// <summary>
/// The NDES Policy Module Interface. When installed against an enterprise CA, NDES generates
/// a password after checking that the user has enrollment permission on the configured NDES
/// templates, both user and machine templates.
/// </summary>
/// <remarks>All methods in this interface are called by server engine.</remarks>
/// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/certpol/nn-certpol-indespolicy">INDESPolicy</seealso>
[Guid("13ca515d-431d-46cc-8c2e-1da269bbd625")] // from CertPol.idl
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[ComImport]
public interface INDESPolicy {
    /// <summary>
    /// Initializes the NDES policy module.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/certpol/nf-certpol-indespolicy-initialize">INDESPolicy::Initialize</seealso>
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Initialize();

    /// <summary>
    /// Uninitializes the NDES policy module.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/certpol/nf-certpol-indespolicy-uninitialize">INDESPolicy::Uninitialize</seealso>
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Uninitialize();

    /// <summary>
    /// Performs the policy decision whether to issue a challenge password to the SCEP client.
    /// </summary>
    /// <param name="pwszTemplate">The template being requested for, as determined by NDES.</param>
    /// <param name="pwszParams">Parameters specific to the policy module implementation.</param>
    /// <returns>
    /// After the user has been authenticated and authorized, the pointer contains the SCEP
    /// challenge password for the user. NDES will free this resource. Must be a pointer to a 2-byte unicode
    /// string containing challenge.
    /// </returns>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/certpol/nf-certpol-indespolicy-generatechallenge">INDESPolicy::GenerateChallenge</seealso>
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    IntPtr GenerateChallenge(
        [MarshalAs(UnmanagedType.LPWStr), In] String pwszTemplate,
        [MarshalAs(UnmanagedType.LPWStr), In] String pwszParams);

    /// <summary>
    /// Verifies the NDES certificate request for submission to the CA.
    /// </summary>
    /// <param name="pctbRequest">The encoded PKCS#10 request.</param>
    /// <param name="pctbSigningCertEncoded">The valid signing certificate for a renewal request. Set to <see cref="IntPtr.Zero"/> for initial request.</param>
    /// <param name="pwszTemplate">The template being requested for, as determined by NDES.</param>
    /// <param name="pwszTransactionId">The SCEP request transaction ID.</param>
    /// <returns><c>true</c> if the challenge is verified; otherwise <c>false</c>.</returns>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/certpol/nf-certpol-indespolicy-verifyrequest">INDESPolicy::VerifyRequest</seealso>
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    Boolean VerifyRequest(
        [In] IntPtr pctbRequest,
        [In] IntPtr pctbSigningCertEncoded,
        [MarshalAs(UnmanagedType.LPWStr), In] String pwszTemplate,
        [MarshalAs(UnmanagedType.LPWStr), In] String pwszTransactionId);

    /// <summary>
    /// Notifies the plug-in of the transaction status of the SCEP certificate request. This method can be used to remove challenge password
    /// from cache on successful issuance to protect from SCEP challenge password replay attacks.
    /// </summary>
    /// <param name="pwszChallenge">The authentication and authorization SCEP challenge password for the user.</param>
    /// <param name="pwszTransactionId">The SCEP request transaction ID.</param>
    /// <param name="disposition">The disposition of the transaction.</param>
    /// <param name="lastHResult">The HRESULT of the last operation.</param>
    /// <param name="pctbIssuedCertEncoded">The requested certificate, if issued.</param>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/certpol/nf-certpol-indespolicy-notify">INDESPolicy::Notify</seealso>
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Notify(
        [MarshalAs(UnmanagedType.LPWStr), In] String pwszChallenge,
        [MarshalAs(UnmanagedType.LPWStr), In] String pwszTransactionId,
        [In] SCEPDisposition disposition,
        [In] Int32 lastHResult,
        [In] IntPtr pctbIssuedCertEncoded);
}

