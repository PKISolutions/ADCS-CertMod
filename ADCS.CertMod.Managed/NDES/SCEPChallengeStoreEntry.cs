#nullable enable
using System;

namespace ADCS.CertMod.Managed.NDES;

/// <summary>
/// Represents SCEP challenge password store (cache) entry.
/// </summary>
/// <param name="Challenge">Specifies the challenge password.</param>
/// <param name="Template">Specifies the template name associated with the current challenge password.</param>
/// <param name="Parameters">Specifies optional challenge password request parameters.</param>
public record SCEPChallengeStoreEntry(String Challenge, String Template, String? Parameters);