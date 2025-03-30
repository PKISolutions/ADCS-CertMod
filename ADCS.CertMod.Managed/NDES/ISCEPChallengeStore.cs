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
    /// <param name="template">Specifies template name the challenge password is requested for.</param>
    /// <param name="parameters">Optional SCEP challenge password parameters.</param>
    /// <returns></returns>
    /// <remarks>
    /// Implementers are responsible to keep track of generated passwords.
    /// </remarks>
    String GetNextChallenge(String template, String? parameters);
    /// <summary>
    /// Called by <see cref="NdesPolicyBase"/> when challenge is successfully consumed.
    /// Implementers are responsible to remove the challenge from the cache.
    /// </summary>
    /// <param name="challenge">Challenge password.</param>
    void ReleaseChallenge(String challenge);
    /// <summary>
    /// Attempts to retrieve a cached and not consumed challenge password.
    /// </summary>
    /// <param name="challenge">Challenge password to retrieve.</param>
    /// <param name="storeEntry">Requested challenge password if found, otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if requested challenge password was found, otherwise <c>false</c>.</returns>
    /// <remarks>This method should be called in <see cref="INDESPolicy.VerifyRequest"/> or
    /// <see cref="NdesPolicyBase.OnVerifyRequest"/> implementation to properly authenticate request.</remarks>
    Boolean TryGetChallenge(String challenge, out SCEPChallengeStoreEntry? storeEntry);
}