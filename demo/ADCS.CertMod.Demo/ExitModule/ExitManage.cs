using System;
using System.Runtime.InteropServices;
using ADCS.CertMod.Demo.Properties;
using ADCS.CertMod.Managed;

namespace ADCS.CertMod.Demo.ExitModule;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("AdcsCertMod_Demo.ExitManage")]
[Guid("434350aa-7cdf-4c78-9973-8f51bf320365")]
public class ExitManage : CertManageModule {
    readonly ILogWriter? _logWriter;

    // MUST NOT remove.
    public ExitManage() { }
    public ExitManage(ILogWriter logger) {
        _logWriter = logger;
        _logWriter.LogDebug(DebugString.EXITMANAGE_CTOR);
    }

    /// <inheritdoc />
    public override Object GetProperty(String strConfig, String strStorageLocation, String strPropertyName, Int32 Flags) {
        _logWriter?.LogDebug(DebugString.EXITMANAGE_GETPROPERTY, strConfig, strStorageLocation, strPropertyName, Flags);
        switch (strPropertyName.ToLower()) {
            case "name":
                return "ADCS.CertMod Demo Exit module";
            case "description":
                return "Demo exit module.";
            case "copyright":
                return "Copyright (c) 2025, Vadims Podans";
            case "file version":
                return "1.0";
            case "product version":
                return "1.0";
            default: return $"Unknown Property: {strPropertyName}";
        }
    }
}