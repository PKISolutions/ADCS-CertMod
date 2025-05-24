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

[HKEY_CLASSES_ROOT\AdcsCertMod_Demo.Policy]
@="ADCS.CertMod.Demo.PolicyModule.Policy"

[HKEY_CLASSES_ROOT\AdcsCertMod_Demo.Policy\CLSID]
@="{{3D61EAC7-D695-4433-9226-2DD6DF344B61}}"

[HKEY_CLASSES_ROOT\CLSID\{{3D61EAC7-D695-4433-9226-2DD6DF344B61}}]
@="ADCS.CertMod.Demo.PolicyModule.Policy"

[HKEY_CLASSES_ROOT\CLSID\{{3D61EAC7-D695-4433-9226-2DD6DF344B61}}\InprocServer32]
@="mscoree.dll"
"ThreadingModel"="Both"
"Class"="ADCS.CertMod.Demo.PolicyModule.Policy"
"Assembly"="ADCS.CertMod.Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{3D61EAC7-D695-4433-9226-2DD6DF344B61}}\InprocServer32\1.0.0.0]
"Class"="ADCS.CertMod.Demo.PolicyModule.Policy"
"Assembly"="ADCS.CertMod.Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{3D61EAC7-D695-4433-9226-2DD6DF344B61}}\ProgId]
@="AdcsCertMod_Demo.Policy"

[HKEY_CLASSES_ROOT\CLSID\{{3D61EAC7-D695-4433-9226-2DD6DF344B61}}\Implemented Categories\{{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}}]

[HKEY_CLASSES_ROOT\AdcsCertMod_Demo.PolicyManage]
@="ADCS.CertMod.Demo.PolicyModule.PolicyManage"

[HKEY_CLASSES_ROOT\AdcsCertMod_Demo.PolicyManage\CLSID]
@="{{203DC9AD-C96B-40D1-A21A-D9922A33AA03}}"

[HKEY_CLASSES_ROOT\CLSID\{{203DC9AD-C96B-40D1-A21A-D9922A33AA03}}]
@="ADCS.CertMod.Demo.PolicyModule.PolicyManage"

[HKEY_CLASSES_ROOT\CLSID\{{203DC9AD-C96B-40D1-A21A-D9922A33AA03}}\InprocServer32]
@="mscoree.dll"
"ThreadingModel"="Both"
"Class"="ADCS.CertMod.Demo.PolicyModule.PolicyManage"
"Assembly"="ADCS.CertMod.Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{203DC9AD-C96B-40D1-A21A-D9922A33AA03}}\InprocServer32\1.0.0.0]
"Class"="ADCS.CertMod.Demo.PolicyModule.PolicyManage"
"Assembly"="ADCS.CertMod.Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{203DC9AD-C96B-40D1-A21A-D9922A33AA03}}\ProgId]
@="AdcsCertMod_Demo.PolicyManage"

[HKEY_CLASSES_ROOT\CLSID\{{203DC9AD-C96B-40D1-A21A-D9922A33AA03}}\Implemented Categories\{{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}}]
'@
$regPolTemplate = "HKLM:\System\CurrentControlSet\Services\CertSvc\Configuration"
$ProgID = "AdcsCertMod_Demo.Policy"
function Register-COM($finalPath) {
    $finalPath = $finalPath -replace "\\","/"
    $tempFile = [System.IO.Path]::GetTempFileName()
    Set-Content -Path $tempFile -Value ($regTemplate -f $finalPath)
    reg import $tempFile | Out-Null
    Remove-Item $tempFile -Force
}
function Copy-Registry() {
    # get active CA node
    $Active = (Get-ItemProperty $regPolTemplate -Name Active).Active
    $script:regPolTemplate += "\$Active\PolicyModules"
    # get active policy module node
    $Active = (Get-ItemProperty $regPolTemplate -Name Active).Active
    if ($Active -ne $ProgID) {
        $src = $regPolTemplate + "\$Active" -replace ":"
        $dest = $regPolTemplate + "\$ProgID" -replace ":"
        reg copy "$src" "$dest" /s /f | Out-Null
        Set-ItemProperty $regPolTemplate\$ProgID -Name "NativeProgID" -Value $Active
    }
}

$ExpectedFileName = "\ADCS.CertMod.Demo.dll"
$finalPath = if ($Path.Exists) {
    $Path.FullName
} else {
    $pwd.Path + $ExpectedFileName
}
if (!(Test-Path $finalPath)) {
    throw New-Object System.IO.FileNotFoundException "Policy module file is not found."
}
if (!$finalPath.EndsWith($ExpectedFileName)) {
    throw New-Object System.ArgumentException "Specified file is not Policy module file."
}
Register-COM $finalPath
if (!$RegisterOnly) {
    Copy-Registry

    if ($AddToCA) {
        Set-ItemProperty $regPolTemplate -Name "Active" -Value $ProgID
        if ($Restart) {
            Restart-Service CertSvc
        }
    }
}