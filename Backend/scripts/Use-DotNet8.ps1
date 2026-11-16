$ErrorActionPreference = 'Stop'
$dotnetCommand = Get-Command dotnet.exe -ErrorAction SilentlyContinue
$installedSdks = if ($dotnetCommand) { & $dotnetCommand.Source --list-sdks } else { @() }
if (-not ($installedSdks -match '^8\.0\.4\d+ ')) {
    $localSdkPath = Join-Path $env:LOCALAPPDATA 'ReMarket\dotnet8'
    if (-not (Test-Path -LiteralPath (Join-Path $localSdkPath 'dotnet.exe'))) {
        throw 'Can cai .NET SDK 8.0.4xx (xem Backend/README.md).'
    }
    $env:DOTNET_ROOT = $localSdkPath
    $env:PATH = "$localSdkPath;$env:PATH"
}
