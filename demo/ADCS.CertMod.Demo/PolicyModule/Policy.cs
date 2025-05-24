using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ADCS.CertMod.Demo.Properties;
using ADCS.CertMod.Managed;
using ADCS.CertMod.Managed.Policy;

namespace ADCS.CertMod.Demo.PolicyModule;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("AdcsCertMod_Demo.Policy")]
[Guid("3d61eac7-d695-4433-9226-2dd6df344b61")]
public class Policy : CertPolicyBase {
    public Policy() : base(new LogWriter("Policy.Demo", LogLevel.Debug, @"C:\")) {
        Logger.LogDebug(DebugString.POLICY_CTOR);
    }
    protected override PolicyModuleAction VerifyRequest(CertServerModule certServer, PolicyModuleAction nativeResult, Boolean bNewRequest) {
        Logger.LogDebug(DebugString.POLICY_VERIFYREQUEST, nativeResult, bNewRequest);
        CertDbRow props = certServer.GetPendingOrFailedProperties();
        foreach (KeyValuePair<String, Object?> keyPair in props) {
            Logger.LogInformation($"{keyPair.Key}: {keyPair.Value}");
        }
        return nativeResult;
    }

    public override void Initialize(String strConfig) {
        Logger.LogDebug(DebugString.POLICY_INITIALIZE, strConfig);
        base.Initialize(strConfig);
    }
    public override void ShutDown() {
        Logger.LogDebug(DebugString.POLICY_SHUTDOWN);
        base.ShutDown();
    }

    public override String GetDescription() {
        Logger.LogDebug(DebugString.POLICY_GETDESCRIPTION);
        return "ADCS.CertMod Demo Policy module";
    }
    public override ICertManageModule GetManageModule() {
        Logger.LogDebug(DebugString.POLICY_GETMANAGEMODULE);
        return new PolicyManage(Logger);
    }
}
