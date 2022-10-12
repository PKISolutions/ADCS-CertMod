using System;
using System.Runtime.InteropServices;

namespace ADCS.CertMod.Managed.Extensions;

static class IntPtrExtensions {
    // adapted version of: https://www.sysadmins.lv/retired-msft-blogs/alejacma/how-to-modify-an-interop-assembly-to-change-the-return-type-of-a-method-vbnet.aspx
    public static Byte[] GetBstrBinary(this IntPtr pvarPropertyValue, ILogWriter logger) {
        OleAut32.VARIANT variant = Marshal.PtrToStructure<OleAut32.VARIANT>(pvarPropertyValue);
        if (variant.vt != OleAut32.VT_BSTR || IntPtr.Zero.Equals(variant.pvRecord)) {
            return default;
        }
        IntPtr pbBstr = variant.pvRecord;
        Int32 cbBstr = Marshal.ReadInt32(pbBstr, -4);
        Byte[] retValue = new Byte[cbBstr];
        Marshal.Copy(pbBstr, retValue, 0, cbBstr);

        return retValue;
    }
}