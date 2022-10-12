using System;
using ADCS.CertMod.Managed.Exit;

namespace ADCS.CertMod.Managed.Policy;

/// <summary>
/// Represents a base implementation of ADCS Certification Authority Policy Module.
/// </summary>
public abstract class CertPolicyBase : ICertPolicy2 {
    const String WINDOWS_POLICY_DEFAULT = "CertificateAuthority_MicrosoftDefault.Policy";

    ICertPolicy defaultPolicy;

    /// <summary>
    /// Initializes a new instance of <strong>CertPolicyBase</strong> class.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// An instance of <strong>Windows Default</strong> cannot be created.
    /// </exception>
    protected CertPolicyBase(ILogWriter logger) {
        Logger = logger;
    }

    /// <summary>
    /// Gets or sets the default policy module ProgID. If this property is not set or invalid,
    /// then Windows Default policy module is used as underlying policy module. This property MUST be
    /// set when non-standard (like FIM CM) is used and you wish to use custom policy module along with
    /// this policy module.
    /// </summary>
    protected String DefaultPolicyProgID { get; set; }
    /// <summary>
    /// Gets or sets the 
    /// </summary>
    protected ILogWriter Logger { get; }

    /// <inheritdoc cref="ICertPolicy.VerifyRequest"/>
    public virtual PolicyModuleAction VerifyRequest(String strConfig, Int32 Context, Int32 bNewRequest, Int32 Flags) {
        return defaultPolicy.VerifyRequest(strConfig, Context, bNewRequest, Flags);
    }
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
            defaultPolicy = Activator.CreateInstance(nativePolicyModuleType) as ICertPolicy;
        } catch (Exception ex) {
            Logger.LogCritical("Unable to create a native policy module instance.");
            Logger.LogCritical(ex, "[CertPolicyBase::Initialize]");
            throw;
        }

        if (defaultPolicy == null) {
            throw new ArgumentException("Native policy module instance is not of type of ICertPolicy.");
        }

        Logger.LogDebug("[CertPolicyBase::Initialize] Initializing native policy module.");
        defaultPolicy.Initialize(strConfig);
        Logger.LogDebug("[CertPolicyBase::Initialize] Native policy module successfully initialized.");
    }
    /// <inheritdoc cref="ICertPolicy2.ShutDown" />
    public virtual void ShutDown() {
        defaultPolicy.ShutDown();
    }
    /// <inheritdoc cref="ICertPolicy.GetDescription"/>
    public abstract String GetDescription();
    /// <inheritdoc />
    public abstract ICertManageModule GetManageModule();
}