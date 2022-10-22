using System;
using CERTCLILib;

namespace ADCS.CertMod.Managed;

abstract class CertServerExitPolicyManaged : ICertServerModule {
    readonly Action<Int32> _setContext;
    readonly Func<String, Int32, IntPtr, Object> _getRequestProperty;
    readonly Func<String, String> _getRequestAttribute;
    readonly Func<String, Int32, IntPtr, Object> _getCertificateProperty;
    readonly Func<String, Int32, IntPtr, Object> _getCertificateExtension;
    readonly Func<Int32> _getCertificateExtensionFlags;
    readonly Action<Int32> _enumerateExtensionsSetup;
    readonly Func<String> _enumerateExtensions;
    readonly Action _enumerateExtensionsClose;
    readonly Action<Int32> _enumerateAttributesSetup;
    readonly Func<String> _enumerateAttributes;
    readonly Action _enumerateAttributesClose;

    protected CertServerExitPolicyManaged(ICertServerExit comClass) {
        Handler = comClass;

        _setContext = comClass.SetContext;
        _getRequestProperty = comClass.GetRequestProperty;
        _getRequestAttribute = comClass.GetRequestAttribute;
        _getCertificateProperty = comClass.GetCertificateProperty;
        _getCertificateExtension = comClass.GetCertificateExtension;
        _getCertificateExtensionFlags = comClass.GetCertificateExtensionFlags;
        _enumerateExtensionsSetup = comClass.EnumerateExtensionsSetup;
        _enumerateExtensions = comClass.EnumerateExtensions;
        _enumerateExtensionsClose = comClass.EnumerateExtensionsClose;
        _enumerateAttributesSetup = comClass.EnumerateAttributesSetup;
        _enumerateAttributes = comClass.EnumerateAttributes;
        _enumerateAttributesClose = comClass.EnumerateAttributesClose;
    }
    protected CertServerExitPolicyManaged(ICertServerPolicy comClass) {
        Handler = comClass;

        _setContext = comClass.SetContext;
        _getRequestProperty = comClass.GetRequestProperty;
        _getRequestAttribute = comClass.GetRequestAttribute;
        _getCertificateProperty = comClass.GetCertificateProperty;
        _getCertificateExtension = comClass.GetCertificateExtension;
        _getCertificateExtensionFlags = comClass.GetCertificateExtensionFlags;
        _enumerateExtensionsSetup = comClass.EnumerateExtensionsSetup;
        _enumerateExtensions = comClass.EnumerateExtensions;
        _enumerateExtensionsClose = comClass.EnumerateExtensionsClose;
        _enumerateAttributesSetup = comClass.EnumerateAttributesSetup;
        _enumerateAttributes = comClass.EnumerateAttributes;
        _enumerateAttributesClose = comClass.EnumerateAttributesClose;
    }

    protected Object Handler { get; }

    public void SetContext(Int32 Context) {
        _setContext.Invoke(Context);
    }
    public void GetRequestProperty(String strPropertyName, Int32 PropertyType, IntPtr pvarPropertyValue) {
        _getRequestProperty.Invoke(strPropertyName, PropertyType, pvarPropertyValue);
    }
    public String GetRequestAttribute(String strAttributeName) {
        return _getRequestAttribute.Invoke(strAttributeName);
    }
    public void GetCertificateProperty(String strPropertyName, Int32 PropertyType, IntPtr pvarPropertyValue) {
        _getCertificateProperty.Invoke(strPropertyName, PropertyType, pvarPropertyValue);
    }
    public void GetCertificateExtension(String strExtensionName, Int32 Type, IntPtr pvarValue) {
        _getCertificateExtension.Invoke(strExtensionName, Type, pvarValue);
    }
    public RequestExtensionFlags GetCertificateExtensionFlags() {
        return (RequestExtensionFlags)_getCertificateExtensionFlags.Invoke();
    }
    public void EnumerateExtensionsSetup(Int32 Flags) {
        _enumerateExtensionsSetup.Invoke(Flags);
    }
    public String EnumerateExtensions() {
        return _enumerateExtensions.Invoke();
    }
    public void EnumerateExtensionsClose() {
        _enumerateExtensionsClose.Invoke();
    }
    public void EnumerateAttributesSetup(Int32 Flags) {
        _enumerateAttributesSetup.Invoke(Flags);
    }
    public String EnumerateAttributes() {
        return _enumerateAttributes.Invoke();
    }
    public void EnumerateAttributesClose() {
        _enumerateAttributesClose.Invoke();
    }
}