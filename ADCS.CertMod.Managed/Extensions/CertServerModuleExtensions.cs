using System;
using System.Runtime.InteropServices;

namespace ADCS.CertMod.Managed.Extensions;

static class CertServerModuleExtensions {
    #region private helpers
    
    public static T GetInSubjectProperty<T>(this ICertServerModule certServerModule, IntPtr pvarPropertyValue, RequestSubjectName subjectName) {
        certServerModule.getScalarProperty(pvarPropertyValue, "Subject." + subjectName, out T retValue);

        return retValue;
    }
    public static T GetOutSubjectProperty<T>(this ICertServerModule certServerModule, IntPtr pvarPropertyValue, RequestSubjectName subjectName) {
        certServerModule.getScalarProperty(pvarPropertyValue, subjectName.ToString(), out T retValue);

        return retValue;
    }

    public static Byte[] GetInSubjectNameBin(this ICertServerModule certServerModule, IntPtr pvarPropertyValue, RequestSubjectName propertyName) {
        return certServerModule.getBinaryProperty(pvarPropertyValue, "Subject." + propertyName);
    }
    public static Byte[] GetOutSubjectNameBin(this ICertServerModule certServerModule, IntPtr pvarPropertyValue, RequestSubjectName propertyName) {
        return certServerModule.getBinaryProperty(pvarPropertyValue, propertyName.ToString());
    }

    public static T GetRequestProperty<T>(this ICertServerModule certServerModule, IntPtr pvarPropertyValue, RequestPropertyName propertyName) {
        certServerModule.getScalarProperty(pvarPropertyValue, propertyName.ToString(), out T retValue);

        return retValue;
    }
    public static Byte[] GetRequestPropertyBin(this ICertServerModule certServerModule, IntPtr pvarPropertyValue, RequestPropertyName propertyName) {
        return certServerModule.getBinaryProperty(pvarPropertyValue, propertyName.ToString());
    }

    public static T GetCertProperty<T>(this ICertServerModule certServerModule, IntPtr pvarPropertyValue, CertificatePropertyName propertyName) {
        certServerModule.getScalarProperty(pvarPropertyValue, propertyName.ToString(), out T retValue, true);

        return retValue;
    }
    public static Byte[] GetCertPropertyBin(this ICertServerModule certServerModule, IntPtr pvarPropertyValue, CertificatePropertyName propertyName) {
        return certServerModule.getBinaryProperty(pvarPropertyValue, propertyName.ToString(), true);
    }

    public static Int32? GetLongProperty(this ICertServerModule certServerModule, IntPtr pvarPropertyValue, String propertyName, Boolean cert = false) {
        if (certServerModule.getScalarProperty(pvarPropertyValue, propertyName, out Int32 retValue, cert)) {
            return retValue;
        }

        return null;
    }
    public static DateTime? GetDateTimeProperty(this ICertServerModule certServerModule, IntPtr pvarPropertyValue, String propertyName, Boolean cert = false) {
        if (certServerModule.getScalarProperty(pvarPropertyValue, propertyName, out DateTime retValue, cert)) {
            return retValue;
        }

        return null;
    }

    static Boolean getScalarProperty<T>(this ICertServerModule certServerModule, IntPtr pvarPropertyValue, String propertyName, out T retValue, Boolean cert = false) {
        retValue = default;
        Type leftType = typeof(T);
        Int32 propType;
        if (leftType == typeof(Int32)) {
            propType = CertSrvH.PROPTYPE_LONG;
        } else if (leftType == typeof(DateTime)) {
            propType = CertSrvH.PROPTYPE_DATE;
        } else if (leftType == typeof(String)) {
            if (propertyName == nameof(RequestPropertyName.RequestAttributes)) { }
            propType = CertSrvH.PROPTYPE_STRING;
        } else {
            return false;
        }

        Int32 hresult = cert
            ? certServerModule.GetCertificateProperty(propertyName, propType, pvarPropertyValue)
            : certServerModule.GetRequestProperty(propertyName, propType, pvarPropertyValue);
        if (hresult != 0) {
            return false;
        }

        retValue = (T)Marshal.GetObjectForNativeVariant(pvarPropertyValue);
        OleAut32.VariantClear(pvarPropertyValue);

        return true;
    }
    static Byte[] getBinaryProperty(this ICertServerModule certServerModule, IntPtr pvarPropertyValue, String propertyName, Boolean cert = false) {
        Int32 hresult = cert
            ? certServerModule.GetCertificateProperty(propertyName, CertSrvH.PROPTYPE_BINARY, pvarPropertyValue)
            : certServerModule.GetRequestProperty(propertyName, CertSrvH.PROPTYPE_BINARY, pvarPropertyValue);

        if (hresult != 0) {
            return default;
        }

        Byte[] retValue = pvarPropertyValue.GetBstrBinary(null);
        OleAut32.VariantClear(pvarPropertyValue);

        return retValue;
    }

    #endregion
}