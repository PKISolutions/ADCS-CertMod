using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace ADCS.CertMod.Managed.Extensions;

/// <summary>
/// Provides useful methods to manage secure strings.
/// </summary>
public static class SecureStringExtensions {
    const DataProtectionScope dpScope = DataProtectionScope.CurrentUser;

    /// <summary>
    /// Encrypts an instance of <seealso cref="SecureString"/> to a DPAPI-protected string.
    /// </summary>
    /// <param name="password">An instance of <seealso cref="SecureString"/>.</param>
    /// <returns>DPAPI-protected string.</returns>
    /// <remarks>This method uses current user context for DPAPI.</remarks>
    public static String EncryptPassword(this SecureString password) {
        Byte[] encryptedData = ProtectedData.Protect(
            Encoding.Unicode.GetBytes(Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(password))),
            null,
            dpScope
        );
        return Convert.ToBase64String(encryptedData);
    }
    /// <summary>
    /// Converts an instance of <seealso cref="SecureString"/> back to plain text.
    /// </summary>
    /// <param name="value">An instance of <seealso cref="SecureString"/>.</param>
    /// <returns>Plain text string.</returns>
    public static String SecureStringToString(this SecureString value) {
        IntPtr bstr = Marshal.SecureStringToBSTR(value);
        try {
            return Marshal.PtrToStringBSTR(bstr).Replace("\0", null);
        } finally {
            Marshal.FreeBSTR(bstr);
        }
    }
    /// <summary>
    /// Decrypts DPAPI-protected string to an instance of <seealso cref="SecureString"/>.
    /// </summary>
    /// <param name="encryptedPassword">DPAPI-encrypted string.</param>
    /// <returns>An instance of <seealso cref="SecureString"/>.</returns>
    /// <remarks>This method uses current user context for DPAPI.</remarks>
    public static SecureString DecryptPassword(this String encryptedPassword) {
        var ss = new SecureString();
        try {
            foreach (Byte b in ProtectedData.Unprotect(Convert.FromBase64String(encryptedPassword), null, dpScope)) {
                ss.AppendChar(Convert.ToChar(b));
            }
        } finally {
            ss.MakeReadOnly();
            GC.Collect();
        }
        return ss;
    }
    /// <summary>
    /// Converts plain text string to secure string.
    /// </summary>
    /// <param name="str">Plain text string.</param>
    /// <returns>An instance of <seealso cref="SecureString"/>.</returns>
    public static SecureString StringToSecureString(this String str) {
        var ss = new SecureString();
        foreach (Char c in str) {
            ss.AppendChar(c);
        }
        ss.MakeReadOnly();
        return ss;
    }
}