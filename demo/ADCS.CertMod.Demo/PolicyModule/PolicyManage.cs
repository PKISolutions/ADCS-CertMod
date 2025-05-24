using System;
using System.Runtime.InteropServices;
using ADCS.CertMod.Demo.Properties;
using ADCS.CertMod.Managed;

namespace ADCS.CertMod.Demo.PolicyModule;

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("AdcsCertMod_Demo.PolicyManage")]
[Guid("203dc9ad-c96b-40d1-a21a-d9922a33aa03")]
public class PolicyManage : CertManageModule {
    readonly ILogWriter? _logWriter;

    public PolicyManage() { }
    public PolicyManage(ILogWriter logWriter) {
        _logWriter = logWriter;
        _logWriter.LogDebug(DebugString.POLICYMANAGE_CTOR2);
    }

    public override Object GetProperty(String strConfig, String strStorageLocation, String strPropertyName, Int32 Flags) {
        _logWriter?.LogDebug(DebugString.POLICYMANAGE_GETPROPERTY, strConfig, strStorageLocation, strPropertyName, Flags);
        switch (strPropertyName.ToLower()) {
            case "name":
                return "ADCS.CertMod Demo Policy module";
            case "description":
                return "Demo policy module.";
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
