using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ADCS.CertMod.Managed.Exit;
using ADCS.CertMod.Managed.Extensions;
using ADCS.CertMod.Managed.Policy;

namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents a communication object between exit or policy module with Certification Authority.
/// </summary>
public class CertServerModule {
    readonly ICertServerModule _certServerModule;
    readonly ILogWriter _logger;

    Boolean isInitialized;
    IntPtr pvarPropertyValue;

    CertServerModule(ILogWriter logger, Boolean certPolicy) {
        if (certPolicy) {
            _certServerModule = new CertServerPolicyManaged();
        } else {
            _certServerModule = new CertServerExitManaged();
        }
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets communication object for exit module.
    /// </summary>
    /// <param name="logger">A logger instance.</param>
    /// <returns>An instance of <see cref="CertServerModule"/>.</returns>
    public static CertServerModule CreateExit(ILogWriter logger) {
        return new CertServerModule(logger, false);
    }
    /// <summary>
    /// Gets communication object for policy module.
    /// </summary>
    /// <param name="logger">A logger instance.</param>
    /// <returns>An instance of <see cref="CertServerModule"/>.</returns>
    public static CertServerModule CreatePolicy(ILogWriter logger) {
        return new CertServerModule(logger, true);
    }

    /// <summary>
    /// Gets request ID for current context. This property is populated only in exit module when
    /// certificate-related event is fired by Certification Authority.
    /// </summary>
    public Int32 RequestID { get; private set; }

    /// <summary>
    /// Initializes a policy module context.
    /// </summary>
    /// <param name="context">Context provided by Certification Authority.</param>
    public void InitializeContext(Int32 context) {
        initializeContext(context);
        RequestID = -1;
    }
    /// <summary>
    /// Initializes an exit module context.
    /// </summary>
    /// <param name="context">Context provided by Certification Authority.</param>
    /// <param name="exitEvent">Exit event provided by Certification Authority.</param>
    public void InitializeEvent(Int32 context, ExitEvents exitEvent) {
        initializeContext(context);
        switch (exitEvent) {
            case ExitEvents.CertIssued:
            case ExitEvents.CertPending:
            case ExitEvents.CertDenied:
            case ExitEvents.CertRevoked:
            case ExitEvents.CertImported:
            case ExitEvents.CertUnrevoked:
            case ExitEvents.CertRetrievePending:
                RequestID = GetRequestID();
                _logger.LogTrace("Request ID: {0}", RequestID);
                break;
            default:
                RequestID = 0;
                break;
        }
    }
    void initializeContext(Int32 context) {
        FinalizeContext();
        pvarPropertyValue = Marshal.AllocHGlobal(32768);
        _certServerModule.SetContext(context);

        isInitialized = true;
    }

    /// <summary>
    /// Finalizes context processing. Must be invoked after finishing <see cref="ICertExit.Notify"/>
    /// and <see cref="ICertPolicy.VerifyRequest"/> method processing.
    /// </summary>
    public void FinalizeContext() {
        if (!IntPtr.Zero.Equals(pvarPropertyValue)) {
            Marshal.FreeHGlobal(pvarPropertyValue);
            pvarPropertyValue = IntPtr.Zero;
        }
        isInitialized = false;
        RequestID = 0;
    }

    /// <summary>
    /// Returns a collection of attributes associated with request context.
    /// </summary>
    /// <returns>A collection of attributes.</returns>
    /// <exception cref="Win32Exception">The method is called before initializing context.</exception>
    public IEnumerable<RequestAttribute> GetRequestAttributes() {
        if (!isInitialized) {
            throw new Win32Exception(unchecked((Int32)0x8000ffff), "Unexpected method call sequence.");
        }
        var retValue = new List<RequestAttribute>();
        if (RequestID == 0) {
            return retValue;
        }

        _certServerModule.EnumerateAttributesSetup(0);
        while (_certServerModule.EnumerateAttributes() is { } attributeName) {
            retValue.Add(new RequestAttribute(RequestID, attributeName, _certServerModule.GetRequestAttribute(attributeName)));
        }
        _certServerModule.EnumerateAttributesClose();

        return retValue;
    }
    /// <summary>
    /// Returns a collection of extensions associated with request context.
    /// </summary>
    /// <returns>A collection of extensions.</returns>
    /// <exception cref="Win32Exception">The method is called before initializing context.</exception>
    public IEnumerable<RequestExtension> GetRequestExtensions() {
        if (!isInitialized) {
            throw new Win32Exception(unchecked((Int32)0x8000ffff), "Unexpected method call sequence.");
        }
        var retValue = new List<RequestExtension>();
        if (RequestID == 0) {
            return retValue;
        }

        _certServerModule.EnumerateExtensionsSetup(0);
        IntPtr pvarValue = Marshal.AllocHGlobal(4096);
        while (_certServerModule.EnumerateExtensions() is { } extensionName) {
            _certServerModule.GetCertificateExtension(extensionName, CertSrvH.PROPTYPE_BINARY, pvarValue);
            RequestExtensionFlags flags = _certServerModule.GetCertificateExtensionFlags();
            retValue.Add(new RequestExtension(RequestID, extensionName, flags, pvarValue.GetBstrBinary()!));
            OleAut32.VariantClear(pvarValue);
        }
        _certServerModule.EnumerateExtensionsClose();
        Marshal.FreeHGlobal(pvarValue);

        return retValue;
    }
    /// <summary>
    /// Returns an underlying implementation of <see cref="ICertServerPolicyManaged"/> interface.
    /// </summary>
    /// <exception cref="InvalidCastException">Current context is not policy module context.</exception>
    /// <returns>Native policy module communicator. Throws exception if current context is exit module.</returns>
    public ICertServerPolicyManaged GetManagedPolicyModule() {
        return (ICertServerPolicyManaged)_certServerModule;
    }

    #region views
    /// <summary>
    /// Returns a dictionary of DB columns for pending or failed request.
    /// </summary>
    /// <returns>Dictionary.</returns>
    public CertDbRow GetPendingOrFailedProperties() {
        var retValue = new CertDbRow(getInProperties());
        retValue.AddRange(getInNames());
        retValue.AddRange(getOutNames());
        retValue.AddRange(getOutProperties(true));

        return retValue;
    }
    /// <summary>
    /// Returns a dictionary of DB columns for issued request.
    /// </summary>
    /// <returns>Dictionary.</returns>
    public CertDbRow GetIssuedProperties() {
        var retValue = new CertDbRow(getInProperties());
        retValue.AddRange(getInNames());
        retValue.AddRange(getOutNames());
        retValue.AddRange(getOutProperties(false));

        return retValue;
    }
    /// <summary>
    /// Returns a dictionary of DB columns for revoked request.
    /// </summary>
    /// <returns>Dictionary.</returns>
    public CertDbRow GetRevokedProperties() {
        var retValue = new CertDbRow();

        retValue.Add("Request_" + nameof(RequestPropertyName.StatusCode), GetStatusCode());
        retValue.Add("Request_" + nameof(RequestPropertyName.Disposition), GetDisposition());
        retValue.Add("Request_" + nameof(RequestPropertyName.DispositionMessage), GetDispositionMessage());
        retValue.Add("Request_" + nameof(RequestPropertyName.RevokedWhen), GetRevokedWhen());
        retValue.Add("Request_" + nameof(RequestPropertyName.RevokedEffectiveWhen), GetRevokedEffectiveWhen());
        retValue.Add("Request_" + nameof(RequestPropertyName.RevokedReason), GetRevokedReason());

        return retValue;
    }

    IDictionary<String, Object?> getInProperties() {
        var retValue = new Dictionary<String, Object?>();

        // In-properties
        retValue.Add("Request_" + nameof(RequestPropertyName.RequestID), GetRequestID());
        retValue.Add("Request_" + nameof(RequestPropertyName.RawRequest), GetRawRequest());
        retValue.Add("Request_" + nameof(RequestPropertyName.RawArchivedKey), GetRawArchivedKey());
        retValue.Add("Request_" + nameof(RequestPropertyName.KeyRecoveryHashes), GetRawKeyRecoveryHashes());
        retValue.Add("Request_" + nameof(RequestPropertyName.RawOldCertificate), GetRawOldCertificate());
        retValue.Add("Request_" + nameof(RequestPropertyName.RequestAttributes), GetRawRequestAttributes());
        retValue.Add("Request_" + nameof(RequestPropertyName.RequestType), GetRequestType());
        retValue.Add("Request_" + nameof(RequestPropertyName.RequestFlags), GetRequestFlags());
        retValue.Add("Request_" + nameof(RequestPropertyName.StatusCode), GetStatusCode());
        retValue.Add("Request_" + nameof(RequestPropertyName.Disposition), GetDisposition());
        retValue.Add("Request_" + nameof(RequestPropertyName.DispositionMessage), GetDispositionMessage());
        retValue.Add("Request_" + nameof(RequestPropertyName.SubmittedWhen), GetSubmittedWhen());
        retValue.Add("Request_" + nameof(RequestPropertyName.ResolvedWhen), GetResolvedWhen());
        retValue.Add("Request_" + nameof(RequestPropertyName.RevokedWhen), GetRevokedWhen());
        retValue.Add("Request_" + nameof(RequestPropertyName.RevokedEffectiveWhen), GetRevokedEffectiveWhen());
        retValue.Add("Request_" + nameof(RequestPropertyName.RevokedReason), GetRevokedReason());
        retValue.Add("Request_" + nameof(RequestPropertyName.RequesterName), GetRequesterName());
        retValue.Add("Request_" + nameof(RequestPropertyName.CallerName), GetCallerName());
        retValue.Add("Request_" + nameof(RequestPropertyName.SignerPolicies), GetSignerPolicies());
        retValue.Add("Request_" + nameof(RequestPropertyName.SignerApplicationPolicies), GetSignerApplicationPolicies());
        retValue.Add("Request_" + nameof(RequestPropertyName.Officer), GetOfficer());
        retValue.Add("Request_" + nameof(RequestPropertyName.AttestationChallenge), GetAttestationChallenge());
        retValue.Add("Request_" + nameof(RequestPropertyName.EndorsementKeyHash), GetEndorsementKeyHash());
        retValue.Add("Request_" + nameof(RequestPropertyName.EndorsementCertificateHash), GetEndorsementCertificateHash());

        retValue.Add(nameof(RequestPropertyName.PublishExpiredCertInCRL), GetPublishExpiredCertInCRL());
        retValue.Add(nameof(RequestPropertyName.RequestID), GetRequestID());

        return retValue;
    }
    IDictionary<String, Object?> getInNames() {
        var retValue = new Dictionary<String, Object?>();

        // In-names
        retValue.Add("Request_" + nameof(RequestSubjectName.DistinguishedName), GetInDistinguishedName());
        retValue.Add("Request_" + nameof(RequestSubjectName.RawName), GetInRawName());
        retValue.Add("Request_" + nameof(RequestSubjectName.Country), GetInCountry());
        retValue.Add("Request_" + nameof(RequestSubjectName.Organization), GetInOrganization());
        retValue.Add("Request_" + nameof(RequestSubjectName.OrgUnit), GetInOrgUnit());
        retValue.Add("Request_" + nameof(RequestSubjectName.CommonName), GetInCommonName());
        retValue.Add("Request_" + nameof(RequestSubjectName.Locality), GetInLocality());
        retValue.Add("Request_" + nameof(RequestSubjectName.State), GetInState());
        retValue.Add("Request_" + nameof(RequestSubjectName.Title), GetInTitle());
        retValue.Add("Request_" + nameof(RequestSubjectName.GivenName), GetInGivenName());
        retValue.Add("Request_" + nameof(RequestSubjectName.Initials), GetInInitials());
        retValue.Add("Request_" + nameof(RequestSubjectName.SurName), GetInSurName());
        retValue.Add("Request_" + nameof(RequestSubjectName.DomainComponent), GetInDomainComponent());
        retValue.Add("Request_" + nameof(RequestSubjectName.EMail), GetInEMail());
        retValue.Add("Request_" + nameof(RequestSubjectName.StreetAddress), GetInStreetAddress());
        retValue.Add("Request_" + nameof(RequestSubjectName.UnstructuredName), GetInUnstructuredName());
        retValue.Add("Request_" + nameof(RequestSubjectName.UnstructuredAddress), GetInUnstructuredAddress());
        retValue.Add("Request_" + nameof(RequestSubjectName.DeviceSerialNumber), GetInDeviceSerialNumber());

        return retValue;
    }
    IDictionary<String, Object?> getOutProperties(Boolean partial) {
        var retValue = new Dictionary<String, Object?>();

        // Out-properties
        if (partial) {
            retValue.Add(nameof(CertificatePropertyName.RawCertificate), null);
            retValue.Add(nameof(CertificatePropertyName.CertificateHash), null);
        } else {
            retValue.Add(nameof(CertificatePropertyName.RawCertificate), GetRawCertificate());
            retValue.Add(nameof(CertificatePropertyName.CertificateHash), GetCertificateHash());
        }

        retValue.Add(nameof(CertificatePropertyName.CertificateTemplate), GetCertificateTemplate());
        retValue.Add(nameof(CertificatePropertyName.EnrollmentFlags), GetEnrollmentFlags());
        retValue.Add(nameof(CertificatePropertyName.GeneralFlags), GetGeneralFlags());
        retValue.Add(nameof(CertificatePropertyName.PrivateKeyFlags), GetPrivateKeyFlags());
        retValue.Add(nameof(CertificatePropertyName.SerialNumber), GetSerialNumber());
        retValue.Add(nameof(CertificatePropertyName.NotBefore), GetNotBefore());
        retValue.Add(nameof(CertificatePropertyName.NotAfter), GetNotAfter());
        retValue.Add(nameof(CertificatePropertyName.SubjectKeyIdentifier), GetSubjectKeyIdentifier());
        retValue.Add(nameof(CertificatePropertyName.RawPublicKey), GetRawPublicKey());
        retValue.Add(nameof(CertificatePropertyName.PublicKeyLength), GetPublicKeyLength());
        retValue.Add(nameof(CertificatePropertyName.PublicKeyAlgorithm), GetPublicKeyAlgorithm());
        retValue.Add(nameof(CertificatePropertyName.RawPublicKeyAlgorithmParameters), GetRawPublicKeyAlgorithmParameters());
        retValue.Add(nameof(CertificatePropertyName.UPN), GetUPN());

        return retValue;
    }
    IDictionary<String, Object?> getOutNames() {
        var retValue = new Dictionary<String, Object?>();

        // Out-names
        retValue.Add(nameof(RequestSubjectName.DistinguishedName), GetOutDistinguishedName());
        retValue.Add(nameof(RequestSubjectName.RawName), GetOutRawName());
        retValue.Add(nameof(RequestSubjectName.Country), GetOutCountry());
        retValue.Add(nameof(RequestSubjectName.Organization), GetOutOrganization());
        retValue.Add(nameof(RequestSubjectName.OrgUnit), GetOutOrgUnit());
        retValue.Add(nameof(RequestSubjectName.CommonName), GetOutCommonName());
        retValue.Add(nameof(RequestSubjectName.Locality), GetOutLocality());
        retValue.Add(nameof(RequestSubjectName.State), GetOutState());
        retValue.Add(nameof(RequestSubjectName.Title), GetOutTitle());
        retValue.Add(nameof(RequestSubjectName.GivenName), GetOutGivenName());
        retValue.Add(nameof(RequestSubjectName.Initials), GetOutInitials());
        retValue.Add(nameof(RequestSubjectName.SurName), GetOutSurName());
        retValue.Add(nameof(RequestSubjectName.DomainComponent), GetOutDomainComponent());
        retValue.Add(nameof(RequestSubjectName.EMail), GetOutEMail());
        retValue.Add(nameof(RequestSubjectName.StreetAddress), GetOutStreetAddress());
        retValue.Add(nameof(RequestSubjectName.UnstructuredName), GetOutUnstructuredName());
        retValue.Add(nameof(RequestSubjectName.UnstructuredAddress), GetOutUnstructuredAddress());
        retValue.Add(nameof(RequestSubjectName.DeviceSerialNumber), GetOutDeviceSerialNumber());

        return retValue;
    }

    #endregion

    #region Request properties
    /// <summary>
    /// Returns a request ID.
    /// </summary>
    /// <returns>Request ID.</returns>
    public Int32 GetRequestID() {
        return _certServerModule.GetRequestProperty<Int32>(pvarPropertyValue, RequestPropertyName.RequestID);
    }
    /// <summary>
    /// Returns binary request.
    /// </summary>
    /// <returns>Binary request.</returns>
    public Byte[]? GetRawRequest() {
        return _certServerModule.GetRequestPropertyBin(pvarPropertyValue, RequestPropertyName.RawRequest);
    }
    /// <summary>
    /// Returns binary archived key.
    /// </summary>
    /// <returns>Binary archived key.</returns>
    public Byte[]? GetRawArchivedKey() {
        return _certServerModule.GetRequestPropertyBin(pvarPropertyValue, RequestPropertyName.RawArchivedKey);
    }
    /// <summary>
    /// Returns new line-delimited string of key recovery agent (KRA) certificate hashes.
    /// </summary>
    /// <returns>A string of hashes.</returns>
    public String? GetRawKeyRecoveryHashes() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.KeyRecoveryHashes);
    }
    /// <summary>
    /// Returns key recovery agent (KRA) certificate hashes.
    /// </summary>
    /// <returns>An array of hashes.</returns>
    public String[] GetKeyRecoveryHashes() {
        String? hashes = _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.KeyRecoveryHashes);
        if (!String.IsNullOrWhiteSpace(hashes)) {
            return hashes!.Split(new Char['\n'], StringSplitOptions.RemoveEmptyEntries);
        }
        
        return [];
    }
    /// <summary>
    /// Returns renewal certificate.
    /// </summary>
    /// <returns>Renewal certificate.</returns>
    public Byte[]? GetRawOldCertificate() {
        return _certServerModule.GetRequestPropertyBin(pvarPropertyValue, RequestPropertyName.RawOldCertificate);
    }
    /// <summary>
    /// Returns raw request attributes as new line-delimited array of name-value pairs.
    /// </summary>
    /// <returns>Raw request attributes.</returns>
    public String? GetRawRequestAttributes() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.RequestAttributes);
    }
    /// <summary>
    /// Returns request type.
    /// </summary>
    /// <returns>Request type.</returns>
    public RequestType? GetRequestType() {
        return (RequestType?)_certServerModule.GetLongProperty(pvarPropertyValue, RequestPropertyName.RequestType.ToString());
    }
    /// <summary>
    /// Returns request flags.
    /// </summary>
    /// <returns>Request flags.</returns>
    public RequestFlags GetRequestFlags() {
        return (RequestFlags)_certServerModule.GetRequestProperty<Int32>(pvarPropertyValue, RequestPropertyName.RequestFlags);
    }
    /// <summary>
    /// Returns HRESULT status code.
    /// </summary>
    /// <returns>Status code.</returns>
    public Int32 GetStatusCode() {
        return _certServerModule.GetRequestProperty<Int32>(pvarPropertyValue, RequestPropertyName.StatusCode);
    }
    /// <summary>
    /// Returns disposition status.
    /// </summary>
    /// <returns>Disposition status.</returns>
    public RequestDisposition GetDisposition() {
        return (RequestDisposition)_certServerModule.GetRequestProperty<Int32>(pvarPropertyValue, RequestPropertyName.Disposition);
    }
    /// <summary>
    /// Returns disposition status message.
    /// </summary>
    /// <returns>Disposition status message.</returns>
    public String? GetDispositionMessage() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.DispositionMessage);
    }
    /// <summary>
    /// Returns request submission timestamp.
    /// </summary>
    /// <returns>Request submission timestamp.</returns>
    public DateTime GetSubmittedWhen() {
        return _certServerModule.GetRequestProperty<DateTime>(pvarPropertyValue, RequestPropertyName.SubmittedWhen);
    }
    /// <summary>
    /// Returns request resolution timestamp.
    /// </summary>
    /// <returns>Request resolution timestamp.</returns>
    public DateTime? GetResolvedWhen() {
        return _certServerModule.GetDateTimeProperty(pvarPropertyValue, RequestPropertyName.ResolvedWhen.ToString());
    }
    /// <summary>
    /// Returns certificate's last revocation timestamp.
    /// </summary>
    /// <returns>Certificate revocation timestamp.</returns>
    public DateTime? GetRevokedWhen() {
        return _certServerModule.GetDateTimeProperty(pvarPropertyValue, RequestPropertyName.RevokedWhen.ToString());
    }
    /// <summary>
    /// Returns certificate's actual revocation timestamp.
    /// </summary>
    /// <returns>Certificate's actual revocation timestamp.</returns>
    public DateTime? GetRevokedEffectiveWhen() {
        return _certServerModule.GetDateTimeProperty(pvarPropertyValue, RequestPropertyName.RevokedEffectiveWhen.ToString());
    }
    /// <summary>
    /// Returns revocation reason.
    /// </summary>
    /// <returns>Revocation reason.</returns>
    public AdcsCrlReason? GetRevokedReason() {
        return (AdcsCrlReason?)_certServerModule.GetLongProperty(pvarPropertyValue, RequestPropertyName.RevokedReason.ToString());
    }
    /// <summary>
    /// Returns requester name.
    /// </summary>
    /// <returns>Requester name.</returns>
    public String? GetRequesterName() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.RequesterName);
    }
    /// <summary>
    /// Returns caller name.
    /// </summary>
    /// <returns>Caller name.</returns>
    public String? GetCallerName() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.CallerName);
    }
    /// <summary>
    /// Returns a new line-delimited array of request co-signing certificate policies.
    /// </summary>
    /// <returns>Co-signing certificate policies.</returns>
    public String? GetSignerPolicies() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.SignerPolicies);
    }
    /// <summary>
    /// Returns a new line-delimited array of request co-signing application policies.
    /// </summary>
    /// <returns>Co-signing application policies.</returns>
    public String? GetSignerApplicationPolicies() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.SignerApplicationPolicies);
    }
    /// <summary>
    /// Returns officer name who resolved request.
    /// </summary>
    /// <returns>Officer name.</returns>
    public String? GetOfficer() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.Officer);
    }
    /// <summary>
    /// Indicates whether current expired and revoked certificate is added in CRL.
    /// </summary>
    /// <returns>
    /// <list type="bullet">
    /// <item><strong>non-zero</strong> -- certificate (when revoked and expired) is added in CRL,</item>
    /// <item><strong>0</strong> or <strong>null</strong> -- certificate (when revoked and expired) is not added in CRL.</item>
    /// </list>
    /// </returns>
    public Int32? GetPublishExpiredCertInCRL() {
        return _certServerModule.GetLongProperty(pvarPropertyValue, RequestPropertyName.PublishExpiredCertInCRL.ToString());
    }
    /// <summary>
    /// Returns binary attestation challenge.
    /// </summary>
    /// <returns>Attestation challenge.</returns>
    public Byte[]? GetAttestationChallenge() {
        return _certServerModule.GetRequestPropertyBin(pvarPropertyValue, RequestPropertyName.AttestationChallenge);
    }
    /// <summary>
    /// Returns endorsement key hash.
    /// </summary>
    /// <returns>Endorsement key hash.</returns>
    public String? GetEndorsementKeyHash() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.EndorsementKeyHash);
    }
    /// <summary>
    /// Returns endorsement certificate hash.
    /// </summary>
    /// <returns>Endorsement certificate hash.</returns>
    public String? GetEndorsementCertificateHash() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.EndorsementCertificateHash);
    }

    #endregion

    #region Request subject properties
    /// <summary>
    /// Returns requested X.500 distinguished name.
    /// </summary>
    /// <returns>Distinguished name.</returns>
    public String? GetInDistinguishedName() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.DistinguishedName);
    }
    /// <summary>
    /// Returns requested ASN.1-encoded X.500 distinguished name.
    /// </summary>
    /// <returns>Encoded distinguished name.</returns>
    public Byte[]? GetInRawName() {
        return _certServerModule.GetInSubjectNameBin(pvarPropertyValue, RequestSubjectName.RawName);
    }
    /// <summary>
    /// Returns requested Country RDN attribute.
    /// </summary>
    /// <returns>Country attribute.</returns>
    public String? GetInCountry() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Country);
    }
    /// <summary>
    /// Returns requested Organization RDN attribute.
    /// </summary>
    /// <returns>Organization attribute.</returns>
    public String? GetInOrganization() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Organization);
    }
    /// <summary>
    /// Returns requested organizational unit RDN attribute.
    /// </summary>
    /// <returns>OU attribute.</returns>
    public String? GetInOrgUnit() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.OrgUnit);
    }
    /// <summary>
    /// Returns requested common name RDN attribute.
    /// </summary>
    /// <returns>CN attribute.</returns>
    public String? GetInCommonName() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.CommonName);
    }
    /// <summary>
    /// Returns requested locality RDN attribute.
    /// </summary>
    /// <returns>Locality attribute.</returns>
    public String? GetInLocality() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Locality);
    }
    /// <summary>
    /// Returns requested StateOrProvince RDN attribute.
    /// </summary>
    /// <returns>StateOrProvince attribute.</returns>
    public String? GetInState() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.State);
    }
    /// <summary>
    /// Returns requested Title RDN attribute.
    /// </summary>
    /// <returns>Title attribute.</returns>
    public String? GetInTitle() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Title);
    }
    /// <summary>
    /// Returns requested GivenName RDN attribute.
    /// </summary>
    /// <returns>Given name attribute.</returns>
    public String? GetInGivenName() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.GivenName);
    }
    /// <summary>
    /// Returns requested Initials RDN attribute.
    /// </summary>
    /// <returns>Initials attribute.</returns>
    public String? GetInInitials() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Initials);
    }
    /// <summary>
    /// Returns requested Surname RDN attribute.
    /// </summary>
    /// <returns>Surname attribute.</returns>
    public String? GetInSurName() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.SurName);
    }
    /// <summary>
    /// Returns requested domain component RDN attribute.
    /// </summary>
    /// <returns>DC attribute.</returns>
    public String? GetInDomainComponent() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.DomainComponent);
    }
    /// <summary>
    /// Returns requested Email RDN attribute.
    /// </summary>
    /// <returns>Email attribute.</returns>
    public String? GetInEMail() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.EMail);
    }
    /// <summary>
    /// Returns requested Street RDN attribute.
    /// </summary>
    /// <returns>Street attribute.</returns>
    public String? GetInStreetAddress() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.StreetAddress);
    }
    /// <summary>
    /// Returns requested unstructured name RDN attribute.
    /// </summary>
    /// <returns>Unstructured name attribute.</returns>
    public String? GetInUnstructuredName() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.UnstructuredName);
    }
    /// <summary>
    /// Returns requested unstructured address RDN attribute.
    /// </summary>
    /// <returns>Unstructured address attribute.</returns>
    public String? GetInUnstructuredAddress() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.UnstructuredAddress);
    }
    /// <summary>
    /// Returns requested device serial number RDN attribute.
    /// </summary>
    /// <returns>Device serial number attribute.</returns>
    public String? GetInDeviceSerialNumber() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.DeviceSerialNumber);
    }

    #endregion

    #region Issued name properties
    /// <summary>
    /// Returns issued X.500 distinguished name.
    /// </summary>
    /// <returns>Distinguished name.</returns>
    public String? GetOutDistinguishedName() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.DistinguishedName);
    }
    /// <summary>
    /// Returns issued ASN.1-encoded X.500 distinguished name.
    /// </summary>
    /// <returns>Encoded distinguished name.</returns>
    public Byte[]? GetOutRawName() {
        return _certServerModule.GetOutSubjectNameBin(pvarPropertyValue, RequestSubjectName.DistinguishedName);
    }
    /// <summary>
    /// Returns issued Country RDN attribute.
    /// </summary>
    /// <returns>Country attribute.</returns>
    public String? GetOutCountry() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Country);
    }
    /// <summary>
    /// Returns issued Organization RDN attribute.
    /// </summary>
    /// <returns>Organization attribute.</returns>
    public String? GetOutOrganization() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Organization);
    }
    /// <summary>
    /// Returns issued organizational unit RDN attribute.
    /// </summary>
    /// <returns>OU attribute.</returns>
    public String? GetOutOrgUnit() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.OrgUnit);
    }
    /// <summary>
    /// Returns issued common name RDN attribute.
    /// </summary>
    /// <returns>CN attribute.</returns>
    public String? GetOutCommonName() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.CommonName);
    }
    /// <summary>
    /// Returns issued locality RDN attribute.
    /// </summary>
    /// <returns>Locality attribute.</returns>
    public String? GetOutLocality() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Locality);
    }
    /// <summary>
    /// Returns issued StateOrProvince RDN attribute.
    /// </summary>
    /// <returns>StateOrProvince attribute.</returns>
    public String? GetOutState() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.State);
    }
    /// <summary>
    /// Returns issued Title RDN attribute.
    /// </summary>
    /// <returns>Title attribute.</returns>
    public String? GetOutTitle() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Title);
    }
    /// <summary>
    /// Returns issued GivenName RDN attribute.
    /// </summary>
    /// <returns>Given name attribute.</returns>
    public String? GetOutGivenName() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.GivenName);
    }
    /// <summary>
    /// Returns issued Initials RDN attribute.
    /// </summary>
    /// <returns>Initials attribute.</returns>
    public String? GetOutInitials() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Initials);
    }
    /// <summary>
    /// Returns issued Surname RDN attribute.
    /// </summary>
    /// <returns>Surname attribute.</returns>
    public String? GetOutSurName() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.SurName);
    }
    /// <summary>
    /// Returns issued domain component RDN attribute.
    /// </summary>
    /// <returns>DC attribute.</returns>
    public String? GetOutDomainComponent() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.DomainComponent);
    }
    /// <summary>
    /// Returns issued Email RDN attribute.
    /// </summary>
    /// <returns>Email attribute.</returns>
    public String? GetOutEMail() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.EMail);
    }
    /// <summary>
    /// Returns issued Street RDN attribute.
    /// </summary>
    /// <returns>Street attribute.</returns>
    public String? GetOutStreetAddress() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.StreetAddress);
    }
    /// <summary>
    /// Returns issued unstructured name RDN attribute.
    /// </summary>
    /// <returns>Unstructured name attribute.</returns>
    public String? GetOutUnstructuredName() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.UnstructuredName);
    }
    /// <summary>
    /// Returns issued unstructured address RDN attribute.
    /// </summary>
    /// <returns>Unstructured address attribute.</returns>
    public String? GetOutUnstructuredAddress() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.UnstructuredAddress);
    }
    /// <summary>
    /// Returns issued device serial number RDN attribute.
    /// </summary>
    /// <returns>Device serial number attribute.</returns>
    public String? GetOutDeviceSerialNumber() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.DeviceSerialNumber);
    }

    #endregion

    #region Certificate properties
    /// <summary>
    /// Returns ASN.1-encoded issued certificate.
    /// </summary>
    /// <returns>Issued certificate.</returns>
    public Byte[]? GetRawCertificate() {
        return _certServerModule.GetCertPropertyBin(pvarPropertyValue, CertificatePropertyName.RawCertificate);
    }
    /// <summary>
    /// Returns issued certificate's SHA1 hash.
    /// </summary>
    /// <returns>Certificate thumbprint.</returns>
    public String? GetCertificateHash() {
        return _certServerModule.GetCertProperty<String>(pvarPropertyValue, CertificatePropertyName.CertificateHash);
    }
    /// <summary>
    /// Returns certificate template name or object identifier (OID).
    /// </summary>
    /// <returns>Certificate template name or OID.</returns>
    public String? GetCertificateTemplate() {
        return _certServerModule.GetCertProperty<String>(pvarPropertyValue, CertificatePropertyName.CertificateTemplate);
    }
    /// <summary>
    /// Returns enrollment flags.
    /// </summary>
    /// <returns>Enrollment flags.</returns>
    public EnrollmentFlags GetEnrollmentFlags() {
        Int32? flags = _certServerModule.GetLongProperty(pvarPropertyValue, CertificatePropertyName.EnrollmentFlags.ToString(), true);
        if (flags.HasValue) {
            return (EnrollmentFlags)flags.Value;
        }

        return default;
    }
    /// <summary>
    /// Returns request general flags.
    /// </summary>
    /// <returns>General flags.</returns>
    public GeneralFlags GetGeneralFlags() {
        Int32? flags = _certServerModule.GetLongProperty(pvarPropertyValue, CertificatePropertyName.GeneralFlags.ToString(), true);
        if (flags.HasValue) {
            return (GeneralFlags)flags.Value;
        }

        return default;
    }
    /// <summary>
    /// Returns private key flags.
    /// </summary>
    /// <returns>Private key flags.</returns>
    public PrivateKeyFlags GetPrivateKeyFlags() {
        Int32? flags = _certServerModule.GetLongProperty(pvarPropertyValue, CertificatePropertyName.PrivateKeyFlags.ToString(), true);
        if (flags.HasValue) {
            return (PrivateKeyFlags)flags.Value;
        }

        return default;
    }
    /// <summary>
    /// Returns the certificate's serial number.
    /// </summary>
    /// <returns>Serial number.</returns>
    public String? GetSerialNumber() {
        return _certServerModule.GetCertProperty<String>(pvarPropertyValue, CertificatePropertyName.SerialNumber);
    }
    /// <summary>
    /// Returns the issued certificate's beginning timestamp.
    /// </summary>
    /// <returns>Certificate beginning timestamp.</returns>
    public DateTime? GetNotBefore() {
        return _certServerModule.GetCertProperty<DateTime>(pvarPropertyValue, CertificatePropertyName.NotBefore);
    }
    /// <summary>
    /// Returns issued certificate's expiration timestamp.
    /// </summary>
    /// <returns>Certificate expiration timestamp.</returns>
    public DateTime? GetNotAfter() {
        return _certServerModule.GetCertProperty<DateTime>(pvarPropertyValue, CertificatePropertyName.NotAfter);
    }
    /// <summary>
    /// Returns the subject key identifier (SKI) string.
    /// </summary>
    /// <returns>Subject key identifier.</returns>
    public String? GetSubjectKeyIdentifier() {
        return _certServerModule.GetCertProperty<String>(pvarPropertyValue, CertificatePropertyName.SubjectKeyIdentifier);
    }
    /// <summary>
    /// Returns ASN.1-encoded public key.
    /// </summary>
    /// <returns>Public key.</returns>
    public Byte[]? GetRawPublicKey() {
        return _certServerModule.GetCertPropertyBin(pvarPropertyValue, CertificatePropertyName.RawPublicKey);
    }
    /// <summary>
    /// Returns public key binary length in bits.
    /// </summary>
    /// <returns>Public key length in bits.</returns>
    public Int32? GetPublicKeyLength() {
        return _certServerModule.GetCertProperty<Int32>(pvarPropertyValue, CertificatePropertyName.PublicKeyLength);
    }
    /// <summary>
    /// Returns public key algorithm name.
    /// </summary>
    /// <returns>Public key algorithm name.</returns>
    public String? GetPublicKeyAlgorithm() {
        return _certServerModule.GetCertProperty<String>(pvarPropertyValue, CertificatePropertyName.PublicKeyAlgorithm);
    }
    /// <summary>
    /// Returns ASN.1-encoded public key algorithm parameters.
    /// </summary>
    /// <returns>Public key algorithm parameters.</returns>
    public Byte[]? GetRawPublicKeyAlgorithmParameters() {
        return _certServerModule.GetCertPropertyBin(pvarPropertyValue, CertificatePropertyName.RawPublicKeyAlgorithmParameters);
    }
    /// <summary>
    /// Returns a user principal name (UPN) from issued certificate.
    /// </summary>
    /// <returns>User principal name.</returns>
    public String? GetUPN() {
        return _certServerModule.GetCertProperty<String>(pvarPropertyValue, CertificatePropertyName.UPN);
    }

    #endregion
}