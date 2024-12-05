using System;

namespace ADCS.CertMod.Managed.Exit;

/// <summary>
/// Represents base implementation of exit module.
/// </summary>
public abstract class CertExitBase : ICertExit2 {
    const Int32 DEFAULT_POOL_SIZE = 32;

    readonly CertServerModulePool _pool;

    /// <summary>
    /// Initializes a new instance of <strong>CertPolicyBase</strong> class.
    /// </summary>
    /// <param name="logger">An instance of custom implementation of <see cref="ILogWriter"/> interface.</param>
    /// <param name="poolSize">Cert Server module pool size. Size must be between 1 and 63. Default is 32.</param>
    /// <exception cref="ArgumentNullException"><strong>logger</strong> parameter is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><strong>poolSize</strong> value is beyond 1-63 range.</exception>
    protected CertExitBase(ILogWriter logger, Int32 poolSize = DEFAULT_POOL_SIZE) {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pool = new CertServerModulePool(poolSize, false, Logger);
    }
    /// <summary>
    /// Initializes a new instance of <strong>CertPolicyBase</strong> class.
    /// </summary>
    /// <param name="logFileName">Log file name to write stream.</param>
    /// <param name="logLevel">Initial log level.</param>
    /// <param name="poolSize">Cert Server module pool size. Size must be between 1 and 63. Default is 32.</param>
    /// <exception cref="ArgumentOutOfRangeException"><strong>poolSize</strong> value is beyond 1-63 range.</exception>
    protected CertExitBase(String logFileName, LogLevel logLevel, Int32 poolSize = DEFAULT_POOL_SIZE) {
        Logger = new LogWriter(logFileName, logLevel);
        _pool = new CertServerModulePool(poolSize, false, Logger);
    }

    /// <summary>
    /// Gets the current stream logger.
    /// </summary>
    protected ILogWriter Logger { get; }
    /// <summary>
    /// <strong>Obsolete.</strong> This member is obsolete.
    /// Use provided instance in <see cref="Notify(CertServerModule, ExitEvents, Int32)"/> overload.
    /// </summary>
    [Obsolete("This member is not thread-safe. Use provided instance in 'Notify(CertServerModule, ExitEvents, Int32)' overload.", true)]
    protected CertServerModule CertServer { get; }

    /// <inheritdoc cref="ICertExit.Initialize" />
    public abstract ExitEvents Initialize(String strConfig);
    /// <inheritdoc cref="ICertExit.Notify" />
    public void Notify(ExitEvents ExitEvent, Int32 Context) {
        CertServerModule certServer = _pool.GetNext();
        certServer.InitializeEvent(Context, ExitEvent);
        try {
            Notify(certServer, ExitEvent, Context);
        } finally {
            _pool.Return(certServer);
        }
    }
    /// <inheritdoc cref="ICertExit.Notify" />
    /// <param name="certServer">An instance of <see cref="CertServerModule"/> class that allows to access request details.</param>
    /// <param name="ExitEvent"><inheritdoc cref="ICertExit.Initialize" path="/param[@name='ExitEvent']"/></param>
    /// <param name="Context"><inheritdoc cref="ICertExit.Initialize" path="/param[@name='Context']"/></param>
    protected abstract void Notify(CertServerModule certServer, ExitEvents ExitEvent, Int32 Context);
    /// <inheritdoc cref="ICertExit.GetDescription" />
    public abstract String GetDescription();
    /// <inheritdoc />
    public abstract ICertManageModule GetManageModule();
}