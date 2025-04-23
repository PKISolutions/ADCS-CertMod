using System;

namespace ADCS.CertMod.Managed;

interface ICertServerModule {
    void SetContext(Int32 Context);
    Int32 GetRequestProperty(String strPropertyName, Int32 PropertyType, IntPtr pvarPropertyValue);
    String GetRequestAttribute(String strAttributeName);
    Int32 GetCertificateProperty(String strPropertyName, Int32 PropertyType, IntPtr pvarPropertyValue);
    void GetCertificateExtension(String strExtensionName, Int32 Type, IntPtr pvarValue);
    RequestExtensionFlags GetCertificateExtensionFlags();
    void EnumerateExtensionsSetup(Int32 Flags);
    String EnumerateExtensions();
    void EnumerateExtensionsClose();
    void EnumerateAttributesSetup(Int32 Flags);
    String EnumerateAttributes();
    void EnumerateAttributesClose();
}