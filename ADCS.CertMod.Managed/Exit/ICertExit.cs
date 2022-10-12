using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ADCS.CertMod.Managed.Exit;

/// <summary>
/// The ICertExit interface provides communications between the Certificate Services server and an exit module.
/// </summary>
/// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/certexit/nn-certexit-icertexit"/>
[Guid("e19ae1a0-7364-11d0-8816-00a0c903b83c")] // from CertExit.idl
[TypeLibType(TypeLibTypeFlags.FDispatchable | TypeLibTypeFlags.FDual)]
[ComImport]
public interface ICertExit {
    /// <summary>
    /// Called by the server engine when it initializes itself.
    /// </summary>
    /// <param name="strConfig">
    /// Represents the name of the certification authority, as entered during Certificate Services setup.
    /// </param>
    /// <returns>A pointer to the value that represents the events for which the exit module requests notification.</returns>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/certexit/nf-certexit-icertexit-initialize">ICertExit::Initialize</seealso>
    [DispId(0x60020000)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    ExitEvents Initialize(
        [MarshalAs(UnmanagedType.BStr), In] String strConfig);

    /// <summary>
    /// Called by the server engine to notify an exit module that an event has occurred.
    /// </summary>
    /// <param name="ExitEvent">A mask that indicates the kind of exit event that has occurred.</param>
    /// <param name="Context">
    ///     Specifies a context handle that can be used to get properties associated with the event from the ICertServerExit interface.
    /// </param>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/certexit/nf-certexit-icertexit-notify">ICertExit::Notify</seealso>
    [DispId(0x60020001)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Notify(
        [In] ExitEvents ExitEvent,
        [In] Int32 Context);

    /// <summary>
    /// Returns a human-readable description of the exit module and its function.
    /// </summary>
    /// <returns>A string that describes the exit module.</returns>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/certexit/nf-certexit-icertexit-getdescription">ICertExit::GetDescription</seealso>
    [DispId(0x60020002)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    [return: MarshalAs(UnmanagedType.BStr)]
    String GetDescription();
}