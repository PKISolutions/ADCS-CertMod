#nullable enable
using System;
using System.Security.Cryptography;
using System.Text;

namespace ADCS.CertMod.Managed.NDES;

/// <summary>
/// Represents a default NDES challenge password generator that uses FIPS-compliant <seealso cref="RNGCryptoServiceProvider"/>
/// to generate cryptographically random challenge password. Produced password is then formatted as a hexadecimal string.
/// </summary>
public class DefaultSCEPChallengeGenerator : ISCEPChallengeGenerator {
    readonly Int16 _challengeLength;
    /// <summary>
    /// Creates a new instance of <seealso cref="DefaultSCEPChallengeGenerator"/> using 
    /// </summary>
    /// <param name="challengeLength">
    /// Challenge password length in bytes. Resulting password will be two times longer because of HEX formatting.
    /// </param>
    public DefaultSCEPChallengeGenerator(Int16 challengeLength = 8) {
        _challengeLength = challengeLength;
    }

    /// <inheritdoc />
    public String GenerateChallenge() {
        Byte[] buffer = new Byte[_challengeLength];
        using var rng = new RNGCryptoServiceProvider();
        rng.GetBytes(buffer);

        var sb = new StringBuilder(buffer.Length * 2);
        foreach (Byte b in buffer) {
            sb.AppendFormat("{0:X2}", b);
        }

        return sb.ToString();
    }
}