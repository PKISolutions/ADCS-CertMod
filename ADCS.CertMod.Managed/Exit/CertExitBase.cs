using System;

namespace ADCS.CertMod.Managed.Exit;

/// <summary>
/// Represents base implementation of exit module.
/// </summary>
public abstract class CertExitBase : ICertExit2 {
    /// <summary>
    /// Initializes a new instance of <strong>CertPolicyBase</strong> class.
    /// </summary>
    /// <param name="logger">An instance of custom implementation of <see cref="ILogWriter"/> interface.</param>
    /// <exception cref="ArgumentNullException"><strong>logger</strong> parameter is null.</exception>
    protected CertExitBase(ILogWriter logger) {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        CertServer = CertServerModule.CreateExit(Logger);
    }
    /// <summary>
    /// Initializes a new instance of <strong>CertPolicyBase</strong> class.
    /// </summary>
    /// <param name="logFileName">Log file name to write stream.</param>
    /// <param name="logLevel">Initial log level.</param>
    protected CertExitBase(String logFileName, LogLevel logLevel) {
        Logger = new LogWriter(logFileName, logLevel);
        CertServer = CertServerModule.CreateExit(Logger);
    }

    /// <summary>
    /// Gets the current stream logger.
    /// </summary>
    protected ILogWriter Logger { get; }
    /// <summary>
    /// Gets the communicator with Certification Authority.
    /// </summary>
    protected CertServerModule CertServer { get; }

    /// <inheritdoc cref="ICertExit.Initialize" />
    public abstract ExitEvents Initialize(String strConfig);
    /// <inheritdoc cref="ICertExit.Notify" />
    public virtual void Notify(ExitEvents ExitEvent, Int32 Context) {
        CertServer.InitializeEvent(Context, ExitEvent);
    }
    /// <inheritdoc cref="ICertExit.GetDescription" />
    public abstract String GetDescription();
    /// <inheritdoc />
    public abstract ICertManageModule GetManageModule();
}