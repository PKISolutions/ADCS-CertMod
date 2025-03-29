#nullable enable
using System;

namespace ADCS.CertMod.Managed.NDES;

/// <summary>
/// Represents NDES challenge password store definition.
/// </summary>
public interface ISCEPChallengeStore {
    /// <summary>
    /// Generates the NDES challenge password.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Implementers are responsible to keep track of generated passwords.
    /// </remarks>
    String GetNextChallenge();
    /// <summary>
    /// Called by <seealso cref="NdesPolicyBase"/> when challenge is successfully consumed.
    /// Implementers are responsible to remove the challenge from the cache.
    /// </summary>
    /// <param name="challenge">Challenge password.</param>
    void ReleaseChallenge(String challenge);
}