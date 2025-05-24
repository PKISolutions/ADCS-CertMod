[CmdletBinding()]
    param(
        [System.IO.FileInfo]$Path,
        [switch]$RegisterOnly,
        [switch]$AddToCA,
        [switch]$Restart
    )
    $ErrorActionPreference = "Stop"

    if (!(Get-Service CertSvc -ErrorAction SilentlyContinue)) {
        Write-Error "Certification Authority is not installed."
        return
    }
    $IsElevated = $false
    foreach ($sid in [Security.Principal.WindowsIdentity]::GetCurrent().Groups) {
        if ($sid.Translate([Security.Principal.SecurityIdentifier]).IsWellKnown([Security.Principal.WellKnownSidType]::BuiltinAdministratorsSid)) {
            $IsElevated = $true
        }
    }

    if (!$IsElevated) {
        Write-Error "Local administrator permissions are required. Ensure that the console is executed in elevated mode and try again."
        return
    }

    $regTemplate = @'
Windows Registry Editor Version 5.00

[HKEY_CLASSES_ROOT\AdcsCertMod_Demo.Exit]
@="ADCS.CertMod.Demo.ExitModule.Exit"

[HKEY_CLASSES_ROOT\AdcsCertMod_Demo.Exit\CLSID]
@="{{34EBA06C-24E0-4068-A049-262E871A6D7B}}"

[HKEY_CLASSES_ROOT\CLSID\{{34EBA06C-24E0-4068-A049-262E871A6D7B}}]
@="ADCS.CertMod.Demo.ExitModule.Exit"

[HKEY_CLASSES_ROOT\CLSID\{{34EBA06C-24E0-4068-A049-262E871A6D7B}}\InprocServer32]
@="mscoree.dll"
"ThreadingModel"="Both"
"Class"="ADCS.CertMod.Demo.ExitModule.Exit"
"Assembly"="ADCS.CertMod.Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{34EBA06C-24E0-4068-A049-262E871A6D7B}}\InprocServer32\1.0.0.0]
"Class"="ADCS.CertMod.Demo.ExitModule.Exit"
"Assembly"="ADCS.CertMod.Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{34EBA06C-24E0-4068-A049-262E871A6D7B}}\ProgId]
@="AdcsCertMod_Demo.Exit"

[HKEY_CLASSES_ROOT\CLSID\{{34EBA06C-24E0-4068-A049-262E871A6D7B}}\Implemented Categories\{{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}}]

[HKEY_CLASSES_ROOT\AdcsCertMod_Demo.ExitManage]
@="ADCS.CertMod.Demo.ExitModule.ExitManage"

[HKEY_CLASSES_ROOT\AdcsCertMod_Demo.ExitManage\CLSID]
@="{{434350AA-7CDF-4C78-9973-8F51BF320365}}"

[HKEY_CLASSES_ROOT\CLSID\{{434350AA-7CDF-4C78-9973-8F51BF320365}}]
@="ADCS.CertMod.Demo.ExitModule.ExitManage"

[HKEY_CLASSES_ROOT\CLSID\{{434350AA-7CDF-4C78-9973-8F51BF320365}}\InprocServer32]
@="mscoree.dll"
"ThreadingModel"="Both"
"Class"="ADCS.CertMod.Demo.ExitModule.ExitManage"
"Assembly"="ADCS.CertMod.Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{434350AA-7CDF-4C78-9973-8F51BF320365}}\InprocServer32\1.0.0.0]
"Class"="ADCS.CertMod.Demo.ExitModule.ExitManage"
"Assembly"="ADCS.CertMod.Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{434350AA-7CDF-4C78-9973-8F51BF320365}}\ProgId]
@="AdcsCertMod_Demo.ExitManage"

[HKEY_CLASSES_ROOT\CLSID\{{434350AA-7CDF-4C78-9973-8F51BF320365}}\Implemented Categories\{{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}}]
'@
$regPolTemplate = "HKLM:\System\CurrentControlSet\Services\CertSvc\Configuration"
$ProgID = "AdcsCertMod_Demo.Exit"
function Register-COM($finalPath) {

    $finalPath = $finalPath -replace "\\","/"
    $tempFile = [System.IO.Path]::GetTempFileName()
    Set-Content -Path $tempFile -Value ($regTemplate -f $finalPath)
    reg import $tempFile | Out-Null
    Remove-Item $tempFile -Force
}
function Add-ToCA() {
    # set active CA node
    $Active = (Get-ItemProperty $regPolTemplate -Name Active).Active
    $script:regPolTemplate += "\$Active\ExitModules"
    # read existing exit modules
    $Active = (Get-ItemProperty $regPolTemplate -Name Active).Active
    # add Demo module if it isn't there
    if ($Active -notcontains $ProgID) {
        [string[]]$Active += $ProgID
        Set-ItemProperty -Path $regPolTemplate -Name "Active" -Value $Active
    }
}

$ExpectedFileName = "\ADCS.CertMod.Demo.dll"
$finalPath = if ($Path.Exists) {
    $Path.FullName
} else {
    $pwd.Path + $ExpectedFileName
}
if (!(Test-Path $finalPath)) {
    throw New-Object System.IO.FileNotFoundException "Exit module file is not found."
}
if (!$finalPath.EndsWith($ExpectedFileName)) {
    throw New-Object System.ArgumentException "Specified file is not Exit module file."
}
Register-COM $finalPath
if (!$RegisterOnly) {
    if ($AddToCA) {
        Add-ToCA
        if ($Restart) {
            Restart-Service CertSvc
        }
    }
}