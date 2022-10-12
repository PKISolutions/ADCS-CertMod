using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using ADCS.CertMod.Managed.Exit;
using ADCS.CertMod.Managed.Extensions;

namespace ADCS.CertMod.Managed;

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

    public static CertServerModule CreateExit(ILogWriter logger) {
        return new CertServerModule(logger, false);
    }
    public static CertServerModule CreatePolicy(ILogWriter logger) {
        return new CertServerModule(logger, true);
    }

    public Int32 RequestID { get; private set; }

    public void InitializeContext(Int32 context) {
        initializeContext(context);
        RequestID = -1;
    }
    public void InitializeEvent(Int32 context, ExitEvents exitEvent) {
        initializeContext(context);
        switch (exitEvent) {
            case ExitEvents.CertIssued:
            case ExitEvents.CertPending:
            case ExitEvents.CertDenied:
            case ExitEvents.CertRevoked:
                RequestID = GetRequestID();
                _logger.LogTrace("Request ID: {0}", RequestID);
                break;
            default:
                RequestID = 0;
                break;
        }
    }
    void initializeContext(Int32 context) {
        pvarPropertyValue = Marshal.AllocHGlobal(32768);
        _certServerModule.SetContext(context);

        isInitialized = true;
    }

    public void FinalizeContext() {
        Marshal.FreeHGlobal(pvarPropertyValue);
        isInitialized = false;
        RequestID = 0;
    }

    public IEnumerable<CertDbRow> GetRequestAttributeCollection() {
        if (!isInitialized) {
            throw new Win32Exception(unchecked((Int32)0x8000ffff), "Unexpected method call sequence.");
        }
        if (RequestID == 0) {
            return null;
        }

        var retValue = new List<CertDbRow>();
        String attributeName;

        _certServerModule.EnumerateAttributesSetup(0);
        while ((attributeName = _certServerModule.EnumerateAttributes()) != null) {
            var attributeRow = new CertDbRow {
                                                 { "AttributeRequestId", RequestID },
                                                 { "AttributeName", attributeName },
                                                 { "AttributeValue", _certServerModule.GetRequestAttribute(attributeName) }
                                             };

            retValue.Add(attributeRow);
        }

        _certServerModule.EnumerateAttributesClose();

        return retValue;
    }
    public IEnumerable<CertDbRow> GetRequestExtensionCollection() {
        if (!isInitialized) {
            throw new Win32Exception(unchecked((Int32)0x8000ffff), "Unexpected method call sequence.");
        }
        if (RequestID == 0) {
            return null;
        }

        var retValue = new List<CertDbRow>();
        String extensionName;

        _certServerModule.EnumerateExtensionsSetup(0);
        IntPtr pvarValue = Marshal.AllocHGlobal(4096);
        while ((extensionName = _certServerModule.EnumerateExtensions()) != null) {
            _certServerModule.GetCertificateExtension(extensionName, CertSrvH.PROPTYPE_BINARY, pvarValue);
            Int32 flags = _certServerModule.GetCertificateExtensionFlags();
            var extensionRow = new CertDbRow {
                                                 { "ExtensionRequestId", RequestID },
                                                 { "ExtensionName", extensionName },
                                                 { "ExtensionFlags", flags },
                                                 { "ExtensionRawValue", pvarValue.GetBstrBinary(_logger) }
                                             };

            retValue.Add(extensionRow);
            OleAut32.VariantClear(pvarValue);
        }

        _certServerModule.EnumerateExtensionsClose();
        Marshal.FreeHGlobal(pvarValue);

        return retValue;
    }

    public ICertServerPolicyManaged GetManagedPolicyModule() {
        return _certServerModule as ICertServerPolicyManaged;
    }

    #region views

    public CertDbRow GetPendingOrFailedProperties() {
        var retValue = new CertDbRow(getInProperties());
        retValue.AddRange(getInNames());
        retValue.AddRange(getOutNames());
        retValue.AddRange(getOutProperties(true));

        return retValue;
    }
    public CertDbRow GetIssuedProperties() {
        var retValue = new CertDbRow(getInProperties());
        retValue.AddRange(getInNames());
        retValue.AddRange(getOutNames());
        retValue.AddRange(getOutProperties(false));

        return retValue;
    }
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

    IDictionary<String, Object> getInProperties() {
        var retValue = new Dictionary<String, Object>();

        // In-properties
        retValue.Add("Request_" + nameof(RequestPropertyName.RequestID), GetRequestID());
        retValue.Add("Request_" + nameof(RequestPropertyName.RawRequest), GetRawRequest());
        retValue.Add("Request_" + nameof(RequestPropertyName.RawArchivedKey), GetRawArchivedKey());
        retValue.Add("Request_" + nameof(RequestPropertyName.KeyRecoveryHashes), GetKeyRecoveryHashes());
        retValue.Add("Request_" + nameof(RequestPropertyName.RawOldCertificate), GetRawOldCertificate());
        retValue.Add("Request_" + nameof(RequestPropertyName.RequestAttributes), GetRequestAttributes());
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
    IDictionary<String, Object> getInNames() {
        var retValue = new Dictionary<String, Object>();

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
    IDictionary<String, Object> getOutProperties(Boolean partial) {
        var retValue = new Dictionary<String, Object>();

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
    IDictionary<String, Object> getOutNames() {
        var retValue = new Dictionary<String, Object>();

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

    public Int32 GetRequestID() {
        return _certServerModule.GetRequestProperty<Int32>(pvarPropertyValue, RequestPropertyName.RequestID);
    }
    public Byte[] GetRawRequest() {
        return _certServerModule.GetRequestPropertyBin(pvarPropertyValue, RequestPropertyName.RawRequest);
    }
    public Byte[] GetRawArchivedKey() {
        return _certServerModule.GetRequestPropertyBin(pvarPropertyValue, RequestPropertyName.RawArchivedKey);
    }
    public String GetKeyRecoveryHashes() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.KeyRecoveryHashes);
    }
    public Byte[] GetRawOldCertificate() {
        return _certServerModule.GetRequestPropertyBin(pvarPropertyValue, RequestPropertyName.RawOldCertificate);
    }
    public String GetRequestAttributes() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.RequestAttributes);
    }
    public Int32? GetRequestType() {
        return _certServerModule.GetLongProperty(pvarPropertyValue, RequestPropertyName.RequestType.ToString());
    }
    public Int32 GetRequestFlags() {
        return _certServerModule.GetRequestProperty<Int32>(pvarPropertyValue, RequestPropertyName.RequestFlags);
    }
    public Int32 GetStatusCode() {
        return _certServerModule.GetRequestProperty<Int32>(pvarPropertyValue, RequestPropertyName.StatusCode);
    }
    public Int32 GetDisposition() {
        return _certServerModule.GetRequestProperty<Int32>(pvarPropertyValue, RequestPropertyName.Disposition);
    }
    public String GetDispositionMessage() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.DispositionMessage);
    }
    public DateTime GetSubmittedWhen() {
        return _certServerModule.GetRequestProperty<DateTime>(pvarPropertyValue, RequestPropertyName.SubmittedWhen);
    }
    public DateTime? GetResolvedWhen() {
        return _certServerModule.GetDateTimeProperty(pvarPropertyValue, RequestPropertyName.ResolvedWhen.ToString());
    }
    public DateTime? GetRevokedWhen() {
        return _certServerModule.GetDateTimeProperty(pvarPropertyValue, RequestPropertyName.RevokedWhen.ToString());
    }
    public DateTime? GetRevokedEffectiveWhen() {
        return _certServerModule.GetDateTimeProperty(pvarPropertyValue, RequestPropertyName.RevokedEffectiveWhen.ToString());
    }
    public Int32? GetRevokedReason() {
        return _certServerModule.GetLongProperty(pvarPropertyValue, RequestPropertyName.RevokedReason.ToString());
    }
    public String GetRequesterName() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.RequesterName);
    }
    public String GetCallerName() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.CallerName);
    }
    public String GetSignerPolicies() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.SignerPolicies);
    }
    public String GetSignerApplicationPolicies() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.SignerApplicationPolicies);
    }
    public String GetOfficer() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.Officer);
    }
    public Int32? GetPublishExpiredCertInCRL() {
        return _certServerModule.GetLongProperty(pvarPropertyValue, RequestPropertyName.PublishExpiredCertInCRL.ToString());
    }
    public Byte[] GetAttestationChallenge() {
        return _certServerModule.GetRequestPropertyBin(pvarPropertyValue, RequestPropertyName.AttestationChallenge);
    }
    public String GetEndorsementKeyHash() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.EndorsementKeyHash);
    }
    public String GetEndorsementCertificateHash() {
        return _certServerModule.GetRequestProperty<String>(pvarPropertyValue, RequestPropertyName.EndorsementCertificateHash);
    }

    #endregion

    #region Request subject properties

    public String GetInDistinguishedName() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.DistinguishedName);
    }
    public Byte[] GetInRawName() {
        return _certServerModule.GetInSubjectNameBin(pvarPropertyValue, RequestSubjectName.RawName);
    }
    public String GetInCountry() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Country);
    }
    public String GetInOrganization() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Organization);
    }
    public String GetInOrgUnit() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.OrgUnit);
    }
    public String GetInCommonName() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.CommonName);
    }
    public String GetInLocality() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Locality);
    }
    public String GetInState() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.State);
    }
    public String GetInTitle() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Title);
    }
    public String GetInGivenName() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.GivenName);
    }
    public String GetInInitials() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Initials);
    }
    public String GetInSurName() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.SurName);
    }
    public String GetInDomainComponent() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.DomainComponent);
    }
    public String GetInEMail() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.EMail);
    }
    public String GetInStreetAddress() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.StreetAddress);
    }
    public String GetInUnstructuredName() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.UnstructuredName);
    }
    public String GetInUnstructuredAddress() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.UnstructuredAddress);
    }
    public String GetInDeviceSerialNumber() {
        return _certServerModule.GetInSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.DeviceSerialNumber);
    }

    #endregion

    #region Issued name properties

    public String GetOutDistinguishedName() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.DistinguishedName);
    }
    public Byte[] GetOutRawName() {
        return _certServerModule.GetOutSubjectNameBin(pvarPropertyValue, RequestSubjectName.DistinguishedName);
    }
    public String GetOutCountry() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Country);
    }
    public String GetOutOrganization() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Organization);
    }
    public String GetOutOrgUnit() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.OrgUnit);
    }
    public String GetOutCommonName() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.CommonName);
    }
    public String GetOutLocality() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Locality);
    }
    public String GetOutState() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.State);
    }
    public String GetOutTitle() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Title);
    }
    public String GetOutGivenName() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.GivenName);
    }
    public String GetOutInitials() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.Initials);
    }
    public String GetOutSurName() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.SurName);
    }
    public String GetOutDomainComponent() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.DomainComponent);
    }
    public String GetOutEMail() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.EMail);
    }
    public String GetOutStreetAddress() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.StreetAddress);
    }
    public String GetOutUnstructuredName() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.UnstructuredName);
    }
    public String GetOutUnstructuredAddress() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.UnstructuredAddress);
    }
    public String GetOutDeviceSerialNumber() {
        return _certServerModule.GetOutSubjectProperty<String>(pvarPropertyValue, RequestSubjectName.DeviceSerialNumber);
    }

    #endregion

    #region Certificate properties

    public Byte[] GetRawCertificate() {
        return _certServerModule.GetCertPropertyBin(pvarPropertyValue, CertificatePropertyName.RawCertificate);
    }
    public String GetCertificateHash() {
        return _certServerModule.GetCertProperty<String>(pvarPropertyValue, CertificatePropertyName.CertificateHash);
    }
    public String GetCertificateTemplate() {
        return _certServerModule.GetCertProperty<String>(pvarPropertyValue, CertificatePropertyName.CertificateTemplate);
    }
    public Int32? GetEnrollmentFlags() {
        return _certServerModule.GetLongProperty(pvarPropertyValue, CertificatePropertyName.EnrollmentFlags.ToString(), true);
    }
    public Int32? GetGeneralFlags() {
        return _certServerModule.GetLongProperty(pvarPropertyValue, CertificatePropertyName.GeneralFlags.ToString(), true);
    }
    public Int32? GetPrivateKeyFlags() {
        return _certServerModule.GetLongProperty(pvarPropertyValue, CertificatePropertyName.PrivateKeyFlags.ToString(), true);
    }
    public String GetSerialNumber() {
        return _certServerModule.GetCertProperty<String>(pvarPropertyValue, CertificatePropertyName.SerialNumber);
    }
    public DateTime GetNotBefore() {
        return _certServerModule.GetCertProperty<DateTime>(pvarPropertyValue, CertificatePropertyName.NotBefore);
    }
    public DateTime GetNotAfter() {
        return _certServerModule.GetCertProperty<DateTime>(pvarPropertyValue, CertificatePropertyName.NotAfter);
    }
    public String GetSubjectKeyIdentifier() {
        return _certServerModule.GetCertProperty<String>(pvarPropertyValue, CertificatePropertyName.SubjectKeyIdentifier);
    }
    public Byte[] GetRawPublicKey() {
        return _certServerModule.GetCertPropertyBin(pvarPropertyValue, CertificatePropertyName.RawPublicKey);
    }
    public Int32 GetPublicKeyLength() {
        return _certServerModule.GetCertProperty<Int32>(pvarPropertyValue, CertificatePropertyName.PublicKeyLength);
    }
    public String GetPublicKeyAlgorithm() {
        return _certServerModule.GetCertProperty<String>(pvarPropertyValue, CertificatePropertyName.PublicKeyAlgorithm);
    }
    public Byte[] GetRawPublicKeyAlgorithmParameters() {
        return _certServerModule.GetCertPropertyBin(pvarPropertyValue, CertificatePropertyName.RawPublicKeyAlgorithmParameters);
    }
    public String GetUPN() {
        return _certServerModule.GetCertProperty<String>(pvarPropertyValue, CertificatePropertyName.UPN);
    }

    #endregion
}