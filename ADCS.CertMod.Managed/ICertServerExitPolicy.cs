using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using ADCS.CertMod.Managed.Extensions;
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
class CertServerExitManaged : CertServerExitPolicyManaged {
    public CertServerExitManaged() : base(new CCertServerExitClass()) { }
}
class CertServerPolicyManaged : CertServerExitPolicyManaged, ICertServerPolicyManaged {
    readonly ICertServerPolicy _certPolicy;

    public CertServerPolicyManaged() : base(new CCertServerPolicyClass()) {
        _certPolicy = (ICertServerPolicy)Handler;
    }

    public void SetCertificateProperty(String strPropertyName, Int32 PropertyType, ref Object pvarPropertyValue) {
        _certPolicy.SetCertificateProperty(strPropertyName, PropertyType, ref pvarPropertyValue);
    }
    public void SetCertificateExtension(X509Extension extension) {
        IntPtr pBstr = Marshal.AllocHGlobal(extension.RawData.Length + 4);
        Marshal.WriteInt32(pBstr, 0, extension.RawData.Length);
        Marshal.Copy(extension.RawData, 0, pBstr + 4, extension.RawData.Length);
        var variant = new OleAut32.VARIANT {
                                               vt = 8, // VT_BSTR
                                               pvRecord = pBstr + 4
                                           };
        IntPtr pvarValue = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(OleAut32.VARIANT)));
        Marshal.StructureToPtr(variant, pvarValue, false);
        Int32 dwCritical = extension.Critical ? 1 : 0;
        try {
            _certPolicy.SetCertificateExtension(extension.Oid.Value, CertSrvH.PROPTYPE_BINARY, dwCritical, pvarValue);
        } finally {
            Marshal.FreeHGlobal(pBstr);
            Marshal.FreeHGlobal(pvarValue);
        }
    }
    public void DisableCertificateExtension(String extensionOid) {
        const Int32 EXTENSION_DISABLE_FLAG = 0x2;
        var variant = new OleAut32.VARIANT {
                                               vt = 0, // VT_EMPTY
                                               pvRecord = IntPtr.Zero
                                           };
        IntPtr pvarValue = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(OleAut32.VARIANT)));
        Marshal.StructureToPtr(variant, pvarValue, false);
        try {
            _certPolicy.SetCertificateExtension(extensionOid, CertSrvH.PROPTYPE_BINARY, EXTENSION_DISABLE_FLAG, pvarValue);
        } finally {
            Marshal.FreeHGlobal(pvarValue);
        }
    }
}