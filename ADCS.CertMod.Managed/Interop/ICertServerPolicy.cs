using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ADCS.CertMod.Managed.Interop;

[Guid("aa000922-ffbe-11cf-8800-00a0c903b83c")] // from certif.h
[TypeLibType(TypeLibTypeFlags.FDual | TypeLibTypeFlags.FDispatchable)]
[ComImport]
public interface ICertServerPolicy {
    [DispId(1610743808)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetContext(
        [In] Int32 Context);

    [DispId(1610743809)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    Int32 GetRequestProperty(
        [MarshalAs(UnmanagedType.BStr), In] String strPropertyName,
        [In] Int32 PropertyType,
        [Out] IntPtr pvarPropertyValue);

    [DispId(1610743810)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [return: MarshalAs(UnmanagedType.BStr)]
    String GetRequestAttribute(
        [MarshalAs(UnmanagedType.BStr), In] String strAttributeName);

    [DispId(1610743811)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    Int32 GetCertificateProperty(
        [MarshalAs(UnmanagedType.BStr), In] String strPropertyName,
        [In] Int32 PropertyType,
        [Out] IntPtr pvarPropertyValue);

    [DispId(1610743812)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetCertificateProperty(
        [MarshalAs(UnmanagedType.BStr), In] String strPropertyName,
        [In] Int32 PropertyType,
        [MarshalAs(UnmanagedType.Struct), In] ref Object pvarPropertyValue);

    [DispId(1610743813)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    Int32 GetCertificateExtension(
        [MarshalAs(UnmanagedType.BStr), In] String strExtensionName,
        [In] Int32 Type,
        [Out] IntPtr pvarValue);

    [DispId(1610743814)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    Int32 GetCertificateExtensionFlags();

    [DispId(1610743815)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void SetCertificateExtension(
        [MarshalAs(UnmanagedType.BStr), In] String strExtensionName,
        [In] Int32 Type,
        [In] Int32 ExtFlags,
        [In] IntPtr pvarValue);

    [DispId(1610743816)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void EnumerateExtensionsSetup(
        [In] Int32 Flags);

    [DispId(1610743817)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [return: MarshalAs(UnmanagedType.BStr)]
    String EnumerateExtensions();

    [DispId(1610743818)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void EnumerateExtensionsClose();

    [DispId(1610743819)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void EnumerateAttributesSetup(
        [In] Int32 Flags);

    [DispId(1610743820)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [return: MarshalAs(UnmanagedType.BStr)]
    String EnumerateAttributes();

    [DispId(1610743821)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void EnumerateAttributesClose();
}