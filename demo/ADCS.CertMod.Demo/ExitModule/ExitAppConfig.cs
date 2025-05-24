using System;
using ADCS.CertMod.Managed;
using Microsoft.Win32;

namespace ADCS.CertMod.Demo.ExitModule;

public class ExitAppConfig(String moduleName, ILogWriter logWriter) : RegistryService(moduleName, CertServerModuleType.Exit) {
    public const String PROP_LOG_LEVEL = "LogLevel";

    public Boolean InitializeConfig() {
        try {
            // initialize registry values if needed
        } catch (Exception ex) {
            logWriter.LogError(ex, "[AppConfig::InitializeConfig]");
            return false;
        }

        return true;
    }

    #region LogLevel

    public LogLevel GetLogLevel() {
        try {
            RegTriplet triplet = GetRecord(PROP_LOG_LEVEL);
            if (triplet is { Type: RegistryValueKind.DWord }) {
                return (LogLevel)triplet.Value;
            }
        } catch { }

        return LogLevel.None;
    }
    public void SetLogLevel(LogLevel logLevel) {
        WriteRecord(new RegTriplet(PROP_LOG_LEVEL, RegistryValueKind.DWord) {
            Value = logLevel
        });
    }

    #endregion
}