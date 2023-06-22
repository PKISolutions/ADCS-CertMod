# Active Directory Certificate Services Exit and Policy module framework

## Download
Use NuGet to download the library and attach to your .NET project:
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
}
```
- `<ModuleName>` is module simple name. The full ProgID must look like `MyCoolExitModule.ExitManage`.
- `<00000000-0000-0000-0000-000000000000>` is a randomly generated UUID that identifies your implementation.
- At a minimum, only `CertManageModule.GetProperty` method must be overriden.

**Note:** angle brackets are used for reference only, they are not used.

### ICertExit2 interface
Create a class that inherits from `CertExitBase` class and define the following attributes:
```C#
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("<ModuleName>.Exit")]
[Guid("<00000000-0000-0000-0000-000000000000>")]
public class MyExitClass : CertExitBase {
<...>
}
```

- `<ModuleName>` is module simple name. The full ProgID must look like `MyCoolExitModule.Exit`.
- `<00000000-0000-0000-0000-000000000000>` is a randomly generated UUID that identifies your implementation.
- `ICertExit2.GetManageModule` returns an instance of `ICertManageModule` implementation (see above).
- a base `CertExitBase.Notify` method shall be called before executing custom code in `Notify` method override.

## Policy module guide
Two interfaces must be implemented and exposed to COM world in order to create an exit module:
- `ICertManageModule`
- `ICertPolicy2`, or inherit from `CertPolicyBase` class directly which provides some base implementation for you.


### ICertManageModule interface
Create a class that inherits from `CertManageModule` class and define the following attributes:
```C#
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("<ModuleName>.PolicyManage")]
[Guid("<00000000-0000-0000-0000-000000000000>")]
public class PolicyManage : CertManageModule {
<...>
}
```
- `<ModuleName>` is module simple name. The full ProgID must look like `MyCoolPolicyModule.PolicyManage`.
- `<00000000-0000-0000-0000-000000000000>` is a randomly generated UUID that identifies your implementation.
- At a minimum, only `CertManageModule.GetProperty` method must be implemented.
