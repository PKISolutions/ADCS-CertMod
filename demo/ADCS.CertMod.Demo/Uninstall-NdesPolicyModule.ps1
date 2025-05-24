[CmdletBinding()]
    param(
        [switch]$Restart
    )
$regTemplate = @'
Windows Registry Editor Version 5.00

[-HKEY_CLASSES_ROOT\CLSID\{246B051D-B7AA-463A-8A7D-D93817A16759}]
[-HKEY_CLASSES_ROOT\AdcsCertMod_Demo.Ndes]
[-HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography\MSCEP\Modules]
'@

# de-register COM components
$tempFile = [System.IO.Path]::GetTempFileName()
Set-Content -Path $tempFile -Value $regTemplate
reg import $tempFile | Out-Null
Remove-Item $tempFile -Force

if ($Restart) {
    Restart-Service w3svc
}