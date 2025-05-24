using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ADCS.CertMod.Demo.Properties;
using ADCS.CertMod.Managed;
using ADCS.CertMod.Managed.Exit;

namespace ADCS.CertMod.Demo.ExitModule;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("AdcsCertMod_Demo.Exit")]
[Guid("34eba06c-24e0-4068-a049-262e871a6d7b")]
public class Exit : CertExitBase {
    readonly ExitAppConfig _appConfig;

    public Exit() : base(new LogWriter("Exit.Demo", LogLevel.Debug, @"C:\")) {
        _appConfig = new ExitAppConfig("AdcsCertMod_Demo.Exit", Logger);
        Logger.LogDebug(DebugString.EXIT_CTOR);
    }

    public override ExitEvents Initialize(String strConfig) {
        Logger.LogDebug(DebugString.EXIT_INITIALIZE, strConfig);
        if (_appConfig.InitializeConfig()) {
            return ExitEvents.AllEvents;
        }

        return ExitEvents.None;
    }

    protected override void Notify(CertServerModule certServer, ExitEvents ExitEvent, Int32 Context) {
        Logger.LogDebug(DebugString.EXIT_NOTIFY, ExitEvent.ToString(), Context);
        
        Logger.LogDebug("[BEGIN] Context: {0}", Context);
        try {
            switch (ExitEvent) {
                case ExitEvents.None:
                    break;
                case ExitEvents.CertIssued:
                case ExitEvents.CertUnrevoked:
                case ExitEvents.CertImported:
                    CertDbRow props = certServer.GetIssuedProperties();
                    foreach (KeyValuePair<String, Object?> keyPair in props) {
                        Logger.LogInformation($"{keyPair.Key}: {keyPair.Value}");
                    }
                    break;
                case ExitEvents.CertPending:
                case ExitEvents.CertDenied:
                    props = certServer.GetPendingOrFailedProperties();
                    foreach (KeyValuePair<String, Object?> keyPair in props) {
                        Logger.LogInformation($"{keyPair.Key}: {keyPair.Value}");
                    }
                    break;
                case ExitEvents.CertRevoked:
                    props = certServer.GetRevokedProperties();
                    foreach (KeyValuePair<String, Object?> keyPair in props) {
                        Logger.LogInformation($"{keyPair.Key}: {keyPair.Value}");
                    }
                    break;
                case ExitEvents.CertRetrievePending:
                    break;
                case ExitEvents.CRLIssued:
                    break;
                case ExitEvents.Shutdown:
                    break;
                case ExitEvents.ServiceStart:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ExitEvent), ExitEvent, null);
            }
        } catch (Exception ex) {
            Logger.LogError(ex, "[EXIT::Notify]");
        }
        Logger.LogDebug("[END] Context: {0}", Context);
    }

    public override String GetDescription() {
        Logger.LogDebug(DebugString.EXIT_GETDESCRIPTION);
        return "ADCS.CertMod Demo Exit module";
    }

    public override ICertManageModule GetManageModule() {
        Logger.LogDebug(DebugString.EXIT_GETMANAGEMODULE);
        return new ExitManage(Logger);
    }
}