using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using ADCS.CertMod.Managed.Extensions;
using ADCS.CertMod.Managed.Interop;

namespace ADCS.CertMod.Managed.Policy;

class CertServerPolicyManaged : CertServerExitPolicyManaged, ICertServerPolicyManaged {
    readonly ICertServerPolicy _certPolicy;

    public CertServerPolicyManaged() : base(CertServerComFactory.CreateCertServerPolicy()) {
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