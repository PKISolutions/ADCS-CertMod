# Active Directory Certificate Services Exit and Policy module framework

## Download
Use [NuGet](https://www.nuget.org/packages/ADCS.CertMod.Managed) to download the library and attach to your .NET project:
```
NuGet\Install-Package ADCS.CertMod.Managed
```

## CI/DI Status:
![image](https://dev.azure.com/pkisolutions/ADCS-CertMod/_apis/build/status/ADCS-CertMod-Nupkg?branchName=master&jobName=Agent%20job%201)
![image](https://vsrm.dev.azure.com/pkisolutions/_apis/public/Release/badge/8c06c171-5a0f-4829-83bc-f52ed00db68c/1/1)
![image](https://img.shields.io/nuget/v/ADCS.CertMod.Managed)

## Online API documentation
[Documentation](https://www.pkisolutions.com/apidocs/certmod)

## Exit Module guide
Two interfaces must be implemented and exposed to COM world in order to create an exit module:
- `ICertManageModule`
- `ICertExit2`

### ICertManageModule interface
Create a class that inherits from `CertManageModule` class and define the following attributes:
```C#
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("<ModuleName>.ExitManage")]
[Guid("<00000000-0000-0000-0000-000000000000>")]
public class ExitManage : CertManageModule {
    <...>
    public override Object GetProperty(String strConfig, String strStorageLocation, String strPropertyName, Int32 Flags) {
        // implementation goes here.
    }
    <...>
}
```
- `<ModuleName>` is module simple name. The full ProgID must look like `MyCoolExitModule.ExitManage`. ProgID and CLR class name are not required to match.
- `<00000000-0000-0000-0000-000000000000>` is a randomly generated UUID that identifies your implementation.
- At a minimum, only `CertManageModule.GetProperty` method must be overriden.

**Note:** angle brackets are used for reference only, they are not used.

### ICertExit2 interface
Create a class that inherits from `CertExitBase` class (which already implements `ICertExit2` interface) and define the following attributes and method overrides:
```C#
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("<ModuleName>.Exit")]
[Guid("<00000000-0000-0000-0000-000000000000>")]
public class MyExitClass : CertExitBase {
    <...>
    // implement public 'Initialize' method
    public override ExitEvents Initialize(String strConfig) {
        // exit module initialization logic goes here
    }
    // implement protected 'Notify' method with your business logic:
    protected override void Notify(CertServerModule certServer, ExitEvents ExitEvent, Int32 Context) {
        // exit module business logic goes here.
    }
    <...>
}
```

- `<ModuleName>` is module simple name. The full ProgID must look like `MyCoolExitModule.Exit`, where `.Exit` suffix is mandatory.
- `<00000000-0000-0000-0000-000000000000>` is a randomly generated UUID that identifies your implementation.
- `ICertExit2.GetManageModule` returns an instance of `ICertManageModule` implementation (see above).

## Policy module guide
Two interfaces must be implemented and exposed to COM world in order to create an exit module:
- `ICertManageModule`
- `ICertPolicy2`, or inherit from `CertPolicyBase` class directly which provides some base implementation for you.


### ICertManageModule interface
See [section above](#icertmanagemodule-interface).

### ICertPolicy2 interface
Create a class that inherits from `CertPolicyBase` class (which already implements `ICertPolicy2` interface) and define the following attributes and method overrides:
```C#
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("<ModuleName>.Policy")]
[Guid("<00000000-0000-0000-0000-000000000000>")]
public class MyPolicyClass : CertPolicyBase {
    <...>
    // implement protected 'VerifyRequest' method with your business logic:
    protected override PolicyModuleAction VerifyRequest(CertServerModule certServer, PolicyModuleAction nativeResult, Boolean bNewRequest) {
        // policy module business logic goes here
    }
    <...>
}
```

- `<ModuleName>` is module simple name. The full ProgID must look like `MyCoolPolicyModule.Policy`, where `.Policy` suffix is mandatory.
- `<00000000-0000-0000-0000-000000000000>` is a randomly generated UUID that identifies your implementation.
- `ICertPolicy2.GetManageModule` returns an instance of `ICertManageModule` implementation (see above).
