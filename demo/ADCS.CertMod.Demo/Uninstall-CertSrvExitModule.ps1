[CmdletBinding()]
param(
        [switch]$Restart
    )
    $regTemplate = @'
Windows Registry Editor Version 5.00

[-HKEY_CLASSES_ROOT\AdcsCertMod_Demo.Exit]
[-HKEY_CLASSES_ROOT\CLSID\{34EBA06C-24E0-4068-A049-262E871A6D7B}]
[-HKEY_CLASSES_ROOT\AdcsCertMod_Demo.ExitManage]
[-HKEY_CLASSES_ROOT\CLSID\{434350AA-7CDF-4C78-9973-8F51BF320365}]
'@

# de-register COM components
$tempFile = [System.IO.Path]::GetTempFileName()
Set-Content -Path $tempFile -Value $regTemplate
reg import $tempFile | Out-Null
Remove-Item $tempFile -Force

$ProgID = "AdcsCertMod_Demo.Exit"

# roll back active CA node
$regPolTemplate = "HKLM:\System\CurrentControlSet\Services\CertSvc\Configuration"
$Active = (Get-ItemProperty $regPolTemplate -Name Active).Active
$regPolTemplate += "\$Active\ExitModules"
$CurrentPolicyModuleRegPath = $regPolTemplate + "\$ProgID"
if (Test-Path $CurrentPolicyModuleRegPath) {
    Remove-Item $CurrentPolicyModuleRegPath
}

$Active = (Get-ItemProperty $regPolTemplate -Name Active).Active
if ($Active -contains $ProgID) {
    [string[]]$Active = $Active | ?{$_ -ne $ProgID}
    Set-ItemProperty -Path $regPolTemplate -Name "Active" -Value $Active
    if ($Restart) {
        Restart-Service CertSvc
    }
}