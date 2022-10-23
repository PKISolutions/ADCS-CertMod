# Active Directory Certificate Services Exit and Policy module framework

## Download
Use NuGet to download the library and attach to your .NET project:
```
NuGet\Install-Package ADCS.CertMod.Managed -Version 1.0.0
```

## Status:
![image](https://dev.azure.com/pkisolutions/ADCS-CertMod/_apis/build/status/ADCS-CertMod-Nupkg?branchName=master&jobName=Agent%20job%201) ![image](https://img.shields.io/nuget/v/ADCS.CertMod.Managed)

## Online API documentation
[Documentation](https://www.pkisolutions.com/apidocs/certmod)

## Exit Module guide
Two interfaces must be implemented and exposed to COM world in order to create an exit module:
- `ICertManageModule`
- `ICertExit2`

### ICertManageModule interface
Create a class that implements `ICertManageModule` and define the following attributes:
```C#
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("<ModuleName>.ExitManage")]
[Guid("<00000000-0000-0000-0000-000000000000>")]
public class ExitManage : ICertManageModule {
<...>
}
```
- `<ModuleName>` is module simple name. The full ProgID must look like `MyCoolExitModule.Exit`.
- `<00000000-0000-0000-0000-000000000000>` is a randomly generated UUID that identifies your implementation.
- At a minimum, only `ICertManageModule.GetProperty` method must be implemented.

### ICertExit2 interface
Create a class that implements `ICertExit2` interface and define the following attributes:
```C#
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("<ModuleName>.Exit")]
[Guid("<00000000-0000-0000-0000-000000000000>")]
public class MyExitClass : ICertExit2 {
<...>
}
```

- `<ModuleName>` is module simple name. The full ProgID must look like `MyCoolExitModule.Exit`.
- `<00000000-0000-0000-0000-000000000000>` is a randomly generated UUID that identifies your implementation.
- `ICertExit2.GetManageModule` returns an instance of `ICertManageModule` implementation (see above).

## Policy module guide
Two interfaces must be implemented and exposed to COM world in order to create an exit module:
- `ICertManageModule`
- `ICertPolicy2`, or inherit from `CertPolicyBase` class directly which provides some base implementation for you.


### ICertManageModule interface
Create a class that implements `ICertManageModule` and define the following attributes:
```C#
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
[ProgId("<ModuleName>.PolicyManage")]
[Guid("<00000000-0000-0000-0000-000000000000>")]
public class PolicyManage : ICertManageModule {
<...>
}
```
- `<ModuleName>` is module simple name. The full ProgID must look like `MyCoolPolicyModule.Policy`.
- `<00000000-0000-0000-0000-000000000000>` is a randomly generated UUID that identifies your implementation.
- At a minimum, only `ICertManageModule.GetProperty` method must be implemented.
