using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace ADCS.CertMod.Managed.NDES;

/// <summary>
/// Represents a base implementation of AD CS Network Device Enrollment Service policy module.
/// </summary>
public abstract class NdesPolicyBase : INDESPolicy {
    readonly String _policyClassName;

    /// <summary>
    /// Initializes a new instance of <strong>NdesPolicyBase</strong> from log writer and SCEP challenge password store implementation.
    /// </summary>
    /// <param name="logWriter">Log writer.</param>
    /// <param name="challengeStore">SCEP challenge password store implementation.</param>
    protected NdesPolicyBase(ILogWriter logWriter, ISCEPChallengeStore challengeStore) {
        _policyClassName = GetType().Name;
        Logger = logWriter;
        ChallengeStore = challengeStore;
        logDebug();
    }

    /// <inheritdoc />
    public void Initialize() {
        logDebug();
        OnInitialize();
    }
    /// <inheritdoc />
    public void Uninitialize() {
        logDebug();
        OnInitialize();
    }
    /// <inheritdoc />
    public IntPtr GenerateChallenge(String pwszTemplate, String pwszParams) {
        logDebug();
        logTrace(".pwszTemplate : {0}", parameters: pwszTemplate);
        logTrace(".pwszParams : {0}", parameters: pwszParams);

        if (String.IsNullOrEmpty(pwszTemplate)) {
            throw new ArgumentException("No such template.");
        }

        String challenge = OnGenerateChallenge(pwszTemplate, pwszParams);

        return Marshal.StringToHGlobalUni(challenge);
    }
    /// <inheritdoc />
    public Boolean VerifyRequest(
        IntPtr pctbRequest,
        IntPtr pctbSigningCertEncoded,
        String pwszTemplate,
        String pwszTransactionId) {
        logDebug();
        logTrace(".pwszTemplate : {0}", parameters: pwszTemplate);
        logTrace(".pwszTransactionId : {0}", parameters: pwszTransactionId);

        dumpBlob(pctbRequest, ".pctbRequest");
        dumpBlob(pctbSigningCertEncoded, ".pctbSigningCertEncoded");

        Byte[]? reqBytes = getBytes(pctbRequest);
        Byte[]? certBytes = getBytes(pctbSigningCertEncoded);
        X509Certificate2? signingCert = null;
        if (certBytes is not null) {
            signingCert = new X509Certificate2(certBytes);
        }

        return OnVerifyRequest(reqBytes, signingCert, pwszTemplate, pwszTransactionId);
    }
    /// <inheritdoc />
    public void Notify(
        String pwszChallenge,
        String pwszTransactionId,
        SCEPDisposition disposition,
        Int32 lastHResult,
        IntPtr pctbIssuedCertEncoded) {
        logDebug();
        logTrace(".pwszChallenge : {0}", parameters: pwszChallenge);
        logTrace(".pwszTransactionId : {0}", parameters: pwszTransactionId);
        logTrace(".disposition : {0}", parameters: disposition);
        logTrace(".lastHResult : {0}", parameters: lastHResult);
        dumpBlob(pctbIssuedCertEncoded, ".pctbIssuedCertEncoded");

        X509Certificate2? cert = null;
        if (!IntPtr.Zero.Equals(pctbIssuedCertEncoded)) {
            cert = new X509Certificate2(getBytes(pctbIssuedCertEncoded));
        }

        OnNotify(pwszChallenge, pwszTransactionId, disposition, lastHResult, cert);
    }

    #region private methods

    void logTrace(String? message = null, [CallerMemberName] String caller = "", params Object[]? parameters) {
        Logger.LogTrace($"{_policyClassName}::{caller} {message}", parameters);
    }
    void logDebug(String? message = null, [CallerMemberName] String caller = "", params Object[]? parameters) {
        Logger.LogDebug($"{_policyClassName}::{caller} {message}", parameters);
    }
    void dumpBlob(IntPtr ptr, String param, [CallerMemberName] String caller = "") {
        logDebug(param, caller);
        if (IntPtr.Zero.Equals(ptr)) {
            return;
        }
        
        var blob = Marshal.PtrToStructure<CRYPTOAPI_BLOB>(ptr);
        logTrace("{0}.cbData : {1}", caller, param, blob.cbData);
        logTrace("{0}.pbData : {1}", caller, param, blob.pbData);

        Byte[] buffer = new Byte[blob.cbData];
        Marshal.Copy(blob.pbData, buffer, 0, buffer.Length);
        logTrace("{0} received the following data:", caller, param);
        logTrace(Convert.ToBase64String(buffer, Base64FormattingOptions.InsertLineBreaks));
    }

    static Byte[]? getBytes(IntPtr ptr) {
        if (IntPtr.Zero.Equals(ptr)) {
            return null;
        }
        var blob = Marshal.PtrToStructure<CRYPTOAPI_BLOB>(ptr);
        Byte[] buffer = new Byte[blob.cbData];
        Marshal.Copy(blob.pbData, buffer, 0, buffer.Length);

        return buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct CRYPTOAPI_BLOB {
        public Int32 cbData;
        public IntPtr pbData;
    }

    #endregion

    #region protected members

    /// <summary>
    /// Gets the current stream logger.
    /// </summary>
    protected ILogWriter Logger { get; }
    /// <summary>
    /// Gets the current challenge password store.
    /// </summary>
    protected ISCEPChallengeStore ChallengeStore { get; }

    /// <summary>
    /// Called by server engine. Implementers use this method to initialize custom policy module. No default implementation provided.
    /// </summary>
    protected virtual void OnInitialize() { }
    /// <summary>
    /// Called by server engine. Implementers use this method to uninitialize custom policy module. No default implementation provided.
    /// </summary>
    protected virtual void OnUninitialize() { }
    /// <summary>
    /// Called by server engine. Implementers use this method to define their own challenge password generator.
    /// </summary>
    /// <param name="template">The template being requested for, as determined by NDES.</param>
    /// <param name="parameters">Parameters specific to the policy module implementation.</param>
    /// <returns>Generated challenge password string.</returns>
    /// <remarks>
    /// This method provides default implementation which calls <see cref="ISCEPChallengeStore.GetNextChallenge"/>.
    /// </remarks>
    protected virtual String OnGenerateChallenge(String template, String parameters) {
        return ChallengeStore.GetNextChallenge(template, parameters);
    }
    /// <inheritdoc cref="INDESPolicy.VerifyRequest" section="summary"/>
    /// <param name="pkcs10Request">The encoded PKCS#10 request.</param>
    /// <param name="signingCertificate">The valid signing certificate for a renewal request. Set to <c>null</c> for initial request.</param>
    /// <param name="template">The template being requested for, as determined by NDES.</param>
    /// <param name="transactionID">The SCEP request transaction ID.</param>
    /// <inheritdoc cref="INDESPolicy.VerifyRequest" section="returns"/>
    /// <remarks>
    /// Implementers shall authenticate the request by comparing challenge password stored in request attributes
    /// (Challenge Password: 1.2.840.113549.1.9.7) against issued and not yet consumed challenge password cache.
    /// <para>
    /// <strong>Note:</strong> challenge password is included in initial requests only (when <strong>signingCertificate</strong>
    /// parameter is <c>null</c>). Renewal requests do not contain challenge password, thus it MUST NOT be checked.
    /// </para>
    /// <para>Implementers MUST NOT remove challenge password from cache in this method override.</para>
    /// </remarks>
    protected abstract Boolean OnVerifyRequest(Byte[]? pkcs10Request, X509Certificate2? signingCertificate, String template, String transactionID);
    /// <inheritdoc cref="INDESPolicy.Notify" section="summary"/>
    /// <param name="challenge">The authentication and authorization SCEP challenge password for the user.</param>
    /// <param name="transactionID">The SCEP request transaction ID.</param>
    /// <param name="disposition">The disposition of the transaction.</param>
    /// <param name="lastHResult">The HRESULT of the last operation.</param>
    /// <param name="issuedCertificate">The requested certificate, if issued.</param>
    /// <remarks>
    /// This method provides default implementation which calls <see cref="ISCEPChallengeStore.ReleaseChallenge"/>.
    /// <para>
    /// <strong>Note:</strong> challenge password is included in initial requests only. Renewal requests do not contain challenge password.
    /// </para>
    /// </remarks>
    protected virtual void OnNotify(String? challenge, String transactionID, SCEPDisposition disposition, Int32 lastHResult, X509Certificate2? issuedCertificate) {
        if (challenge is null) {
            return;
        }
        
        ChallengeStore.ReleaseChallenge(challenge);
    }

    #endregion
}