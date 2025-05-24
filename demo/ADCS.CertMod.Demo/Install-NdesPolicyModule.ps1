[CmdletBinding()]
    param(
        [System.IO.File]$Path,
        [switch]$AddToNDES,
        [switch]$Restart
    )
    $ErrorActionPreference = "Stop"

    if (!(Get-WindowsFeature ADCS-Device-Enrollment).Installed) {
        Write-Error "NDES is not installed."
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

[HKEY_CLASSES_ROOT\AdcsCertMod_Demo.Ndes]
@="ADCS.CertMod.Demo.NdesModule.NdesPolicyModule"

[HKEY_CLASSES_ROOT\AdcsCertMod_Demo.Ndes\CLSID]
@="{{246B051D-B7AA-463A-8A7D-D93817A16759}}"

[HKEY_CLASSES_ROOT\CLSID\{{246B051D-B7AA-463A-8A7D-D93817A16759}}]
@="ADCS.CertMod.Demo.NdesModule.NdesPolicyModule"

[HKEY_CLASSES_ROOT\CLSID\{{246B051D-B7AA-463A-8A7D-D93817A16759}}\InprocServer32]
@="mscoree.dll"
"ThreadingModel"="Both"
"Class"="ADCS.CertMod.Demo.NdesModule.NdesPolicyModule"
"Assembly"="ADCS.CertMod.Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{246B051D-B7AA-463A-8A7D-D93817A16759}}\InprocServer32\1.0.0.0]
"Class"="ADCS.CertMod.Demo.NdesModule.NdesPolicyModule"
"Assembly"="ADCS.CertMod.Demo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6406f24a7e84ddc5"
"RuntimeVersion"="v4.0.30319"
"CodeBase"="file:///{0}"

[HKEY_CLASSES_ROOT\CLSID\{{246B051D-B7AA-463A-8A7D-D93817A16759}}\ProgId]
@="AdcsCertMod_Demo.Ndes"

[HKEY_CLASSES_ROOT\CLSID\{{246B051D-B7AA-463A-8A7D-D93817A16759}}\Implemented Categories\{{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}}]
'@
$regPolTemplate = "HKLM:\SOFTWARE\Microsoft\Cryptography\MSCEP"
$ProgID = "AdcsCertMod_Demo.Ndes"
function Register-COM($finalPath) {
    $finalPath = $finalPath -replace "\\","/"
    $tempFile = [System.IO.Path]::GetTempFileName()
    [System.IO.File]::WriteAllText($tempFile, ($regTemplate -f $finalPath))
    [void](reg import $tempFile)
    Remove-Item $tempFile -Force
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

if ($AddToNDES) {
    New-Item $regPolTemplate\Modules -ErrorAction SilentlyContinue | Out-Null
    Set-ItemProperty $regPolTemplate\Modules -Name "Policy" -Value $ProgID
    if ($Restart) {
        Restart-Service w3svc
    }
}