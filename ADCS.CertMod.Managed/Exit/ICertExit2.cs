using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ADCS.CertMod.Managed.Exit;

[Guid("0abf484b-d049-464d-a7ed-552e7529b0ff")] // from CertExit.idl
[TypeLibType(TypeLibTypeFlags.FDispatchable | TypeLibTypeFlags.FDual)]
[ComImport]
public interface ICertExit2 : ICertExit {
    /// <inheritdoc cref="ICertExit.Initialize"/>
    [DispId(0x60020000)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    new ExitEvents Initialize(
        [MarshalAs(UnmanagedType.BStr), In] String strConfig);

    /// <inheritdoc cref="ICertExit.Notify"/>
    [DispId(0x60020001)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    new void Notify(
        [In] ExitEvents ExitEvent,
        [In] Int32 Context);

    /// <inheritdoc cref="ICertExit.GetDescription"/>
    [DispId(0x60020002)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [return: MarshalAs(UnmanagedType.BStr)]
    new String GetDescription();

    /// <summary>
    /// Called by the server engine to retrieve the object that allows the Certification Authority to configure the exit module.
    /// </summary>
    /// <returns>An instance of <see cref="ICertManageModule"/>.</returns>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/certexit/nf-certexit-icertexit2-getmanagemodule">ICertPolicy2::GetManageModule</seealso>
    [DispId(0x60030000)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [return: MarshalAs(UnmanagedType.Interface)]
    ICertManageModule GetManageModule();
}