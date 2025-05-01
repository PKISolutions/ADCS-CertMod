#nullable enable
using System;

namespace ADCS.CertMod.Managed.NDES;

/// <summary>
/// Represents NDES challenge password generator.
/// </summary>
public interface ISCEPChallengeGenerator {
    /// <summary>
    /// Generates NDES challenge password string.
    /// </summary>
    /// <returns>NDES challenge password.</returns>
    String GenerateChallenge();
}