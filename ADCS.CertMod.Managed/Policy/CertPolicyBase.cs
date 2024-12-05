using System;

namespace ADCS.CertMod.Managed.Policy;

/// <summary>
/// Represents a base implementation of ADCS Certification Authority Policy Module.
/// </summary>
public abstract class CertPolicyBase : ICertPolicy2 {
    const String WINDOWS_POLICY_DEFAULT = "CertificateAuthority_MicrosoftDefault.Policy";
    const Int32 DEFAULT_POOL_SIZE = 32;

    readonly CertServerModulePool _pool;

    Action funcShutdown;
    Action<String> funcInitialize;
    Func<String, Int32, Int32, Int32, PolicyModuleAction> funcVerifyRequest;

    /// <summary>
    /// Initializes a new instance of <strong>CertPolicyBase</strong> class.
    /// </summary>
    /// <param name="logger">An instance of custom implementation of <see cref="ILogWriter"/> interface.</param>
    /// <param name="poolSize">Cert Server module pool size. Size must be between 1 and 63. Default is 32.</param>
    /// <exception cref="ArgumentNullException"><strong>logger</strong> parameter is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><strong>poolSize</strong> value is beyond 1-63 range.</exception>
    protected CertPolicyBase(ILogWriter logger, Int32 poolSize = DEFAULT_POOL_SIZE) {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pool = new CertServerModulePool(poolSize, true, Logger);
    }
    /// <summary>
    /// Initializes a new instance of <strong>CertPolicyBase</strong> class.
    /// </summary>
    /// <param name="logFileName">Log file name to write stream.</param>
    /// <param name="logLevel">Initial log level.</param>
    /// <param name="poolSize">Cert Server module pool size. Size must be between 1 and 63. Default is 32.</param>
    /// <exception cref="ArgumentOutOfRangeException"><strong>poolSize</strong> value is beyond 1-63 range.</exception>
    protected CertPolicyBase(String logFileName, LogLevel logLevel, Int32 poolSize = DEFAULT_POOL_SIZE) {
        Logger = new LogWriter(logFileName, logLevel);
        _pool = new CertServerModulePool(poolSize, true, Logger);
    }

    /// <summary>
    /// Gets or sets the default policy module ProgID. If this property is not set or invalid,
    /// then Windows Default policy module is used as underlying policy module. This property MUST be
    /// set when non-standard (like FIM CM) is used and you wish to use custom policy module along with
    /// this policy module.
    /// </summary>
    protected String DefaultPolicyProgID { get; set; }
    /// <summary>
    /// Gets the current stream logger.
    /// </summary>
    protected ILogWriter Logger { get; }
    /// <summary>
    /// <strong>Obsolete.</strong> This member is obsolete.
    /// Use provided instance in <see cref="VerifyRequest(CertServerModule, PolicyModuleAction, Boolean)"/> overload.
    /// </summary>
    [Obsolete("This member is not thread-safe. Use provided instance in 'VerifyRequest(CertServerModule, PolicyModuleAction, Boolean)' overload.", true)]
    protected CertServerModule CertServer { get; }

    /// <inheritdoc cref="ICertPolicy.VerifyRequest"/>
    public PolicyModuleAction VerifyRequest(String strConfig, Int32 Context, Int32 bNewRequest, Int32 Flags) {
        CertServerModule certServer = _pool.GetNext();
        certServer.InitializeContext(Context);

        PolicyModuleAction nativeResult = funcVerifyRequest.Invoke(strConfig, Context, bNewRequest, Flags);
        try {
            return VerifyRequest(certServer, nativeResult, bNewRequest > 0);
        } finally {
            _pool.Return(certServer);
        }
    }

    /// <inheritdoc cref="ICertPolicy.VerifyRequest"/>
    /// <param name="certServer">An instance of <see cref="CertServerModule"/> class that allows to access request details.</param>
    /// <param name="nativeResult">Native policy module result for current request.</param>
    /// <param name="bNewRequest">A boolean that indicates if request is new request, or renewal request.</param>
    protected abstract PolicyModuleAction VerifyRequest(CertServerModule certServer, PolicyModuleAction nativeResult, Boolean bNewRequest);

    /// <inheritdoc cref="ICertPolicy.Initialize"/>
    public virtual void Initialize(String strConfig) {
        Type nativePolicyModuleType;
        String nativePolicyModuleName = DefaultPolicyProgID ?? "Windows Default";
        Logger.LogDebug($"[CertPolicyBase::Initialize] Native policy module ProgID: {nativePolicyModuleName}");
        if (String.IsNullOrWhiteSpace(DefaultPolicyProgID)) {
            nativePolicyModuleType = Type.GetTypeFromProgID(WINDOWS_POLICY_DEFAULT, false);
            if (nativePolicyModuleType == null) {
                throw new ArgumentException("COM class is not registered.");
            }
        } else {
            nativePolicyModuleType = Type.GetTypeFromProgID(DefaultPolicyProgID, false);
            if (nativePolicyModuleType == null) {
                Logger.LogError("[CertPolicyBase::Initialize] Unable to discover native policy module with ProgID: {0}", DefaultPolicyProgID);
                // fallback to Windows Default policy module.
                nativePolicyModuleType = Type.GetTypeFromProgID(WINDOWS_POLICY_DEFAULT, false);
                if (nativePolicyModuleType == null) {
                    throw new ArgumentException("COM class is not registered.");
                }
            }
        }

        try {
            Object instance = Activator.CreateInstance(nativePolicyModuleType);
            if (instance == null) {
                throw new ArgumentException("Native policy module instance is null.");
            }
            // we can cast native COM-based policy module to our ICertPolicy interface.
            // we cannot cast native .NET-based policy module to our ICertPolicy interface
            // because of how CLR distinguishes types.
            // In addition, we cannot create delegates from COM object using Delegate.CreateDelegate,
            // as the result we have to use different approaches for different policy module implementations.
            if (instance is ICertPolicy certPolicy) {
                funcShutdown = certPolicy.ShutDown;
                funcInitialize = certPolicy.Initialize;
                funcVerifyRequest = certPolicy.VerifyRequest;
            } else {
                // we cannot cast native .NET-based policy module to our ICertPolicy interface
                funcShutdown = (Action)Delegate.CreateDelegate(typeof(Action), instance, nameof(ICertPolicy.ShutDown));
                funcInitialize = (Action<String>)Delegate.CreateDelegate(typeof(Action<String>), instance, nameof(ICertPolicy.Initialize));
                funcVerifyRequest = (Func<String, Int32, Int32, Int32, PolicyModuleAction>)Delegate.CreateDelegate(
                    typeof(Func<String, Int32, Int32, Int32, PolicyModuleAction>),
                    instance,
                    nameof(ICertPolicy.VerifyRequest));
            }
        } catch (Exception ex) {
            Logger.LogCritical("Unable to create a native policy module instance.");
            Logger.LogCritical(ex, "[CertPolicyBase::Initialize]");
            throw;
        }

        Logger.LogDebug("[CertPolicyBase::Initialize] Initializing native policy module.");
        funcInitialize.Invoke(strConfig);
        Logger.LogDebug("[CertPolicyBase::Initialize] Native policy module successfully initialized.");
    }
    /// <inheritdoc cref="ICertPolicy2.ShutDown" />
    public virtual void ShutDown() {
        funcShutdown.Invoke();
    }
    /// <inheritdoc cref="ICertPolicy.GetDescription"/>
    public abstract String GetDescription();
    /// <inheritdoc />
    public abstract ICertManageModule GetManageModule();
}