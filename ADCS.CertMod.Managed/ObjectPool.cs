using System;
using System.Threading;

namespace ADCS.CertMod.Managed;

/// <summary>
/// Represents generic thread-safe object pool.
/// </summary>
/// <typeparam name="T">Pool object type.</typeparam>
abstract class ObjectPool<T> where T : class {
    // .NET upper limit is 64, however in STA apps, it is 63. Stick with the lowest supported.
    // .NET limit: https://github.com/microsoft/referencesource/blob/51cf7850defa8a17d815b4700b67116e3fa283c2/mscorlib/system/threading/waithandle.cs#L41
    // STA limit: https://github.com/dotnet/runtime/pull/1647
    const Int32 POOL_UPPER_BOUND = 63;
    readonly Mutex _hMutexAlterState;
    readonly ManualResetEvent[] _events;
    readonly T[] _pool;

    /// <summary>
    /// Initializes a new instance of <strong>ObjectPool</strong> from pool size and a delegate to create pool items.
    /// </summary>
    /// <param name="size">Pool size. Must be between 1 and 63.</param>
    /// <param name="objectGenerator">An optional delegate to create pool items. If absent, a parameterless constructor is used instead.</param>
    /// <exception cref="ArgumentOutOfRangeException">Invalid pool size. Must be between 1 and 63.</exception>
    protected ObjectPool(Int32 size, Func<T>? objectGenerator = null) {
        if (size is < 1 or > POOL_UPPER_BOUND) {
            throw new ArgumentOutOfRangeException(nameof(size), size, "Invalid pool size. Must be between 1 and 63.");
        }
        _hMutexAlterState = new Mutex(false, null);
        _events = new ManualResetEvent[size];
        _pool = new T[size];
        for (Int32 i = 0; i < _pool.Length; i++) {
            _pool[i] = objectGenerator is null
                ? Activator.CreateInstance<T>()
                : objectGenerator();
            _events[i] = new ManualResetEvent(true);
        }
    }

    /// <summary>
    /// Executes custom action before the object is returned to the pool.
    /// </summary>
    /// <param name="obj">Object being returned.</param>
    protected virtual void OnObjectReturn(T obj) { }

    /// <summary>
    /// Retrieves next available instance from object pool.
    /// </summary>
    /// <returns></returns>
    /// <remarks>After object is no longer in use, it MUST be released by calling <see cref="Return"/> method.</remarks>
    public T GetNext() {
        T? result = null;
        if (_hMutexAlterState.WaitOne()) {
            try {
                // wait indefinitely
                Int32 num = WaitHandle.WaitAny(_events, -1);
                _events[num].Reset();
                result = _pool[num];
            } finally {
                _hMutexAlterState.ReleaseMutex();
            }
        }

        return result;
    }
    /// <summary>
    /// Returns object to pool.
    /// </summary>
    /// <param name="obj">Object to return.</param>
    /// <exception cref="InvalidOperationException">
    /// The object instance doesn't belong to current pool. Only objects created by <see cref="GetNext"/> method can be returned to the pool.
    /// </exception>
    public void Return(T obj) {
        for (Int32 i = 0; i < _pool.Length; i++) {
            if (ReferenceEquals(_pool[i], obj)) {
                OnObjectReturn(_pool[i]);
                _events[i].Set();

                return;
            }
        }

        throw new InvalidOperationException("The supplied object doesn't belong to current object pool.");
    }
}