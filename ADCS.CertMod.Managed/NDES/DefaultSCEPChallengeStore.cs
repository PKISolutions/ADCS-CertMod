#nullable enable
using System;
using System.Collections.Concurrent;

namespace ADCS.CertMod.Managed.NDES;

public class DefaultSCEPChallengeStore : ISCEPChallengeStore {
    readonly ConcurrentDictionary<String, String> _store = [];
    readonly Int32 _storageLimit;
    readonly ISCEPChallengeGenerator _challengeGenerator;

    /// <summary>
    /// Initializes a new instance of <seealso cref="DefaultSCEPChallengeStore"/> from challenge generator
    /// and optional store size limit.
    /// </summary>
    /// <param name="challengeGenerator">SCEP challenge password implementation.</param>
    /// <param name="StorageLimit">Optional SCEP challenge password limit. Default is 0, which means no limits.</param>
    public DefaultSCEPChallengeStore(ISCEPChallengeGenerator challengeGenerator, Int32 StorageLimit = 0) {
        _challengeGenerator = challengeGenerator;
        _storageLimit = StorageLimit;
    }

    /// <inheritdoc />
    public String GetNextChallenge() {
        lock (_store) {
            if (_storageLimit > 0 && _store.Count == _storageLimit) {
                throw new ArgumentException("Store is full. Cannot generate more passwords.");
            }

            String challenge = _challengeGenerator.GenerateChallenge();
            _store[challenge] = "";

            return challenge;
        }
    }
    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    /// Supplied challenge password doesn't exist in internal store. Either, it was already released or never
    /// generated using <seealso cref="GetNextChallenge"/> method call.
    /// </exception>
    public void ReleaseChallenge(String challenge) {
        if (!_store.TryRemove(challenge, out _)) {
            throw new ArgumentException("Challenge being released was never generated");
        }
    }
}