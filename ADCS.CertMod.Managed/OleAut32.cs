using System;
using System.Runtime.InteropServices;

namespace ADCS.CertMod.Managed;

static class OleAut32 {
    public const Int16 VT_BSTR = 0x8;

    [DllImport("OleAut32.dll", SetLastError = true)]
    public static extern Int32 VariantClear(IntPtr pvarg);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct VARIANT {
        public Int16 vt;
        public Int16 wReserved1;
        public Int16 wReserved2;
        public Int16 wReserved3;
        public IntPtr pvRecord;
    }
}