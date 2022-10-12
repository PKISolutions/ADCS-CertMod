using System;
using System.Security.Cryptography.X509Certificates;

namespace ADCS.CertMod.Managed;

public interface ICertServerPolicyManaged {
    void SetCertificateProperty(String strPropertyName, Int32 PropertyType, ref Object pvarPropertyValue);
    void SetCertificateExtension(X509Extension extension);
    void DisableCertificateExtension(String extensionOid);
}