$ErrorActionPreference = 'Stop'
try {
    Add-Type -AssemblyName System.Windows.Forms
    Add-Type -AssemblyName System.Drawing
    Add-Type -Path (Join-Path $PSScriptRoot 'Cat.cs') -ReferencedAssemblies System.Windows.Forms,System.Drawing
    [DesktopCat]::Run()
} catch {
    Write-Host ('Spusteni selhalo: ' + $_.Exception.Message) -ForegroundColor Red
    Read-Host 'Stisknete Enter'
    exit 1
}
