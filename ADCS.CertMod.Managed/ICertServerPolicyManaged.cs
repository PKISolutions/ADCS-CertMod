using System;
using System.Security.Cryptography.X509Certificates;

namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents communication interface between policy module and Certification Authority.
/// </summary>
public interface ICertServerPolicyManaged {
    /// <summary>
    /// Sets certificate property.
    /// </summary>
    /// <param name="strPropertyName">Property name to set.</param>
    /// <param name="PropertyType">Value type.</param>
    /// <param name="pvarPropertyValue">Value to set.</param>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/certif/nf-certif-icertserverpolicy-setcertificateproperty"/>
    void SetCertificateProperty(String strPropertyName, Int32 PropertyType, ref Object pvarPropertyValue);
    /// <summary>
    /// Adds or overwrites certificate extension in issued certificate.
    /// </summary>
    /// <param name="extension">Initialized extension to write.</param>
    void SetCertificateExtension(X509Extension extension);
    /// <summary>
    /// Disables certificate extension in issued certificate.
    /// </summary>
    /// <param name="extensionOid">Extension OID to disable.</param>
    void DisableCertificateExtension(String extensionOid);
}