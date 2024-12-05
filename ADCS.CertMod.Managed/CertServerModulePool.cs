#nullable enable
using System;

namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents a pool for <see cref="CertServerModule"/> objects.
/// </summary>
/// <param name="size">Pool size. Must be between 1 and 63.</param>
/// <param name="isPolicyModule">Specifies whether the module is policy or exit.</param>
/// <param name="logger">An instance of custom implementation of <see cref="ILogWriter"/> interface.</param>
class CertServerModulePool(Int32 size, Boolean isPolicyModule, ILogWriter logger)
    : ObjectPool<CertServerModule>(size, () => isPolicyModule
                                             ? CertServerModule.CreatePolicy(logger)
                                             : CertServerModule.CreateExit(logger)) {
    protected override void OnObjectReturn(CertServerModule obj) {
        // finalize context in case caller forget to finalize. This method is idempotent, so safe to call multiple times.
        obj.FinalizeContext();
    }
}