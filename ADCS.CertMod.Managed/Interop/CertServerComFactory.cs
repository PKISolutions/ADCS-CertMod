using System.Runtime.InteropServices;
// ReSharper disable SuspiciousTypeConversion.Global

namespace ADCS.CertMod.Managed.Interop;

/// <summary>
/// Represents CertServer* COM class factory.
/// </summary>
static class CertServerComFactory {
    /// <summary>
    /// Creates an instance of <strong>ICertServerExit</strong> COM interface.
    /// </summary>
    /// <returns>ICertServerExit.</returns>
    public static ICertServerExit CreateCertServerExit() {
        return (ICertServerExit)new CCertServerExitClass();
    }
    /// <summary>
    /// Creates an instance of <strong>ICertServerPolicy</strong> COM interface.
    /// </summary>
    /// <returns>ICertServerPolicy.</returns>
    public static ICertServerPolicy CreateCertServerPolicy() {
        return (ICertServerPolicy)new CCertServerPolicyClass();
    }

    [Guid("4c4a5e40-732c-11d0-8816-00a0c903b83c")]
    [TypeLibType(TypeLibTypeFlags.FCanCreate)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComImport]
    class CCertServerExitClass;

    [Guid("aa000926-ffbe-11cf-8800-00a0c903b83c")]
    [TypeLibType(TypeLibTypeFlags.FCanCreate)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComImport]
    class CCertServerPolicyClass;
}