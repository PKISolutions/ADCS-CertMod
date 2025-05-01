using System;
using System.Collections.Concurrent;

namespace ADCS.CertMod.Managed.NDES;

/// <summary>
/// Represents a thread-safe default implementation of SCEP challenge password store/cache. Key properties are:
/// <list type="bullet">
///     <item>Can be limited in size. By default, cache size is unlimited.</item>
///     <item>Transient. Erased when NDES application pool is recycled or stopped.</item>
///     <item>Do not bind challenge password to template.</item>
/// </list>
/// </summary>
public class DefaultSCEPChallengeStore : ISCEPChallengeStore {
    readonly ConcurrentDictionary<String, SCEPChallengeStoreEntry> _store = [];
    readonly Int32 _storageLimit;
    readonly ISCEPChallengeGenerator _challengeGenerator;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultSCEPChallengeStore"/> from challenge generator
    /// and optional store size limit.
    /// </summary>
    /// <param name="challengeGenerator">SCEP challenge password implementation.</param>
    /// <param name="storageLimit">Optional SCEP challenge password limit. Default is 0, which means no limits.</param>
    public DefaultSCEPChallengeStore(ISCEPChallengeGenerator challengeGenerator, Int32 storageLimit = 0) {
        _challengeGenerator = challengeGenerator;
        _storageLimit = storageLimit;
    }

    /// <inheritdoc />
    public String GetNextChallenge(String template, String? parameters) {
        lock (_store) {
            if (_storageLimit > 0 && _store.Count == _storageLimit) {
                throw new ArgumentException("Store is full. Cannot generate more passwords.");
            }

            String challenge = _challengeGenerator.GenerateChallenge();
            _store[challenge] = new SCEPChallengeStoreEntry(challenge, template, parameters);

            return challenge;
        }
    }
    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    /// Supplied challenge password doesn't exist in internal store. Either, it was already released or never
    /// generated using <see cref="GetNextChallenge"/> method call.
    /// </exception>
    public void ReleaseChallenge(String challenge) {
        if (!_store.TryRemove(challenge, out _)) {
            throw new ArgumentException("Challenge being released was never generated");
        }
    }

    /// <inheritdoc />
    public Boolean TryGetChallenge(String challenge, out SCEPChallengeStoreEntry? storeEntry) {
        return _store.TryGetValue(challenge, out storeEntry);
    }
}