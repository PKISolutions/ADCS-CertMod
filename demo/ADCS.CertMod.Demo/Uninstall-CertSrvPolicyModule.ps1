[CmdletBinding()]
    param(
        [switch]$Restart
    )
$regTemplate = @'
Windows Registry Editor Version 5.00

[-HKEY_CLASSES_ROOT\PKISolutions_SID.Policy]
[-HKEY_CLASSES_ROOT\CLSID\{3D61EAC7-D695-4433-9226-2DD6DF344B61}]

[-HKEY_CLASSES_ROOT\PKISolutions_SID.PolicyManage]
[-HKEY_CLASSES_ROOT\CLSID\{203DC9AD-C96B-40D1-A21A-D9922A33AA03}]
'@

# de-register COM components
$tempFile = [System.IO.Path]::GetTempFileName()
Set-Content -Path $tempFile -Value $regTemplate
reg import $tempFile | Out-Null
Remove-Item $tempFile -Force

$ProgID = "AdcsCertMod_Demo.Policy"

# roll back active CA node
$regPolTemplate = "HKLM:\System\CurrentControlSet\Services\CertSvc\Configuration"
$Active = (Get-ItemProperty $regPolTemplate -Name Active).Active
$CurrentPolicyModuleRegPath = $regPolTemplate + "\$Active\PolicyModules\$ProgID"
if (!(Test-Path $CurrentPolicyModuleRegPath)) {
    return
}
$nativeProgID = (Get-ItemProperty -Path $CurrentPolicyModuleRegPath -Name "NativeProgID" -ErrorAction SilentlyContinue).NativeProgID
if (!$nativeProgID) {
    $nativeProgID = "CertificateAuthority_MicrosoftDefault.Policy"
}
Set-ItemProperty -Path ($regPolTemplate + "\$Active\PolicyModules") -Name "Active" -Value $nativeProgID
Remove-Item $CurrentPolicyModuleRegPath -Force
if ($Restart) {
    Restart-Service CertSvc
}